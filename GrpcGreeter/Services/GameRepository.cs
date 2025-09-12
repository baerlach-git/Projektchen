namespace GrpcGreeter.Services;

using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using Bogus;
using Models;
using FakerClasses;
using Grpcgreeter.Helpers;

public class GameRepository
{
  //I'm not sure if this is the best exception to throw
  private readonly string _connectionString = 
    Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Connection string not set in environment variables.");


  private IDbConnection Connection => new MySqlConnection(_connectionString);

  public async Task<IEnumerable<GameDto>> GetGamesWithRatingsAsync()
  {
    var query = @"
      SELECT 
        g.Id AS Id, g.Name AS Name, g.ReleaseDate AS ReleaseDate,
        p.Name AS Publisher, 
        d.Name AS Developer, 
        GROUP_CONCAT(DISTINCT pl.Name) AS Platform,
        GROUP_CONCAT(DISTINCT gen.Name) AS Genre,
        AVG(r.Rating) AS AverageRating
      FROM Game g
      JOIN Publisher p ON g.PublisherId = p.Id
      JOIN Developer d ON g.DeveloperId = d.Id
      JOIN Game_Platform gm ON g.Id = gm.GameId
      JOIN Platform pl ON gm.PlatformId = pl.Id
      JOIN Game_Genre gg ON g.Id = gg.GameId
      JOIN Genre gen ON gen.ID = gg.GenreId
      JOIN GameRating r ON g.Id = r.GameId
      GROUP BY Id, Name, ReleaseDate, Publisher, Developer;
      ";
    using var db = Connection;
    var result = await db.QueryAsync<GameDto>(query);
    return result;
  }

  public async Task<bool> GameExistsAsync(uint gameId)
  {
    using var db = Connection;
    var sql = "SELECT COUNT(*) FROM Game WHERE Id = @Id";
    var result = await db.ExecuteScalarAsync<int>(sql, new { Id = gameId });
    return result > 0;
  }

  public async Task<bool> RatingExistsAsync(uint gameId, string Ip)
  {
    using var db = Connection;
    var sql = @"
      SELECT COUNT(*) 
      FROM Game g 
      JOIN GameRating r 
      ON g.Id = r.GameId
      WHERE g.Id = @Id AND r.Ip = @Ip";
    var result = await db.ExecuteScalarAsync<int>(sql, new { Id = gameId, Ip });
    return result > 0;

  }

  public async Task<int> AddRatingAsync(GameRatingUpsertData rating)
  {
    using var db = Connection;
    var sql = @"
      INSERT INTO GameRating (GameId, Ip, Rating)
      VALUES (@GameId, @Ip, @Rating)";

    var addRatingResponse = await db.ExecuteAsync(sql, rating);
    return addRatingResponse;
  }

  public async Task<int> UpdateRatingAsync(GameRatingUpsertData rating)
  {
    using var db = Connection;
    var sql = @"
      UPDATE GameRatingg 
      SET Rating = @Rating
      WHERE Ip = @Id";
    var updateResponse = await db.ExecuteAsync(sql, rating);
    return updateResponse;

  }
  
  
  

  private async Task<int> GetGameCountAsync()
  {
    using var db = Connection;
    var gameCount = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Game");
    return gameCount;
  }

  private async Task<int> GetGameRatingCount()
  {
    using var db = Connection;
    var gameRatingCount = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM GameRating");
    return gameRatingCount;
  }

  private async Task<int> GetGamePlatformRelationCount()
  {
    using var db = Connection;
    var gamePlatformRelationCount = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Game_Platform");
    return gamePlatformRelationCount;
  }

  private async Task<int> GetGameGenreRelationCount()
  {
    using var db = Connection;
    var gameGenreRelationCount = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Game_Genre");
    return gameGenreRelationCount;
  }

  private async Task<List<int>> GetPublisherIdsAsync()
  {
    using var db = Connection;
    var publisherIds = (await db.QueryAsync<int>("SELECT PublisherId FROM Publisher")).ToList();
    return publisherIds;
  }

  private async Task<List<int>> GetDeveloperIdsAsync()
  {
    using var db = Connection;
    var developerIds = (await db.QueryAsync<int>("SELECT Id FROM Developer")).ToList();  
    return developerIds;
  }

  private async Task<List<int>> GetGameIdsAsync()
  {
    using var db = Connection;
    var gameIds = (await db.QueryAsync<int>("SELECT Id FROM Game")).ToList();
    return gameIds;
  }

  private async Task<List<int>> GetPlatformIdsAsync()
  {
    using var db = Connection;
    var platformIds = (await db.QueryAsync<int>("SELECT Id FROM Platform")).ToList();
    return platformIds;
  }

  private async Task<int> InsertGames(List<GameInsertData> games)
  {
    using var db = Connection;
    var insertGames = @"
      INSERT INTO Game (Name, ReleaseDate, PublisherId, DeveloperId) 
      VALUES (@Name, @ReleaseDate, @PublisherId, @DeveloperId)";
    var insertGamesResponse = await db.ExecuteAsync(insertGames, games);
    return insertGamesResponse;
  }

  private async Task<int> InsertGameRatings(List<GameRatingUpsertData> ratings)
  {
    using var db = Connection;
    var insertRatings = @"
      INSERT INTO GameRating (GameId, Ip, Rating) 
      VALUES (@GameId, @Ip, @Rating);";
    var insertGameRatingsResponse = await db.ExecuteAsync(insertRatings, ratings);
    return insertGameRatingsResponse;
  }


  public async Task SeedDatabaseAsync()
  {
    using var db = Connection;
    var gameCount = await GetGameCountAsync();
    
    if (gameCount == 0)
    {
      var publisherIds = await GetPublisherIdsAsync();
      var developerIds = await GetDeveloperIdsAsync();
      
      var gameFaker = new Faker<GameInsertData>()
        .RuleFor(g => g.Name, f => f.Commerce.ProductName())
        .RuleFor(g => g.ReleaseDate, f => 
            f.Random.Int(SeedDataConfig.MinReleaseDate, SeedDataConfig.MaxReleaseDate))
        .RuleFor(g => g.PublisherId, (f, g) => f.PickRandom(publisherIds))
        .RuleFor(g => g.DeveloperId, (f, g) => f.PickRandom(developerIds));

      var fakeGames = gameFaker.Generate(SeedDataConfig.FakeGameAmount);
      //TODO check for return value and throw an exception if it 0 even though FakeGameAmount is not 0
      await InsertGames(fakeGames);
    }


    var ratingCount = await GetGameRatingCount();
    if (ratingCount == 0)
    {
      var gameIds = await GetGameIdsAsync();
      var ratingFaker = new Faker<GameRatingUpsertData>()
        .RuleFor(r => r.GameId, (f => f.PickRandom(gameIds)))
        .RuleFor(r => r.Ip, f => f.Internet.Ip())
        .RuleFor(r => r.Rating, (f => (uint)f.Random.Int(SeedDataConfig.MinRating, SeedDataConfig.MaxRating)));

      var ratings = ratingFaker.Generate(SeedDataConfig.FakeRatingAmount);
      await InsertGameRatings(ratings);
    }

    var gamePlatformRelationCount = await GetGamePlatformRelationCount();
    if (gamePlatformRelationCount == 0)
    {
      var gameIds = await GetGameIdsAsync();
      var platformIds = GetPlatformIdsAsync();

      //Each game should have at least one platform to ensure this I'm using two fakers as a workaround.
      //the first one uses deterministic game ids, the second one random ones
      var gameId = 0;
      var deterministicGamePlatformFaker = new Faker<FakeGamePlatformRelation>()
      .RuleFor(gp => gp.GameId, f => gameIds[gameId++])
      .RuleFor(gp => gp.PlatformId, f => f.PickRandom(platformIds));

      var deterministicFakeGamePlatformRelations = deterministicGamePlatformFaker.Generate(SeedDataConfig.FakeGameAmount);
      var deterministicInsert = "INSERT INTO Game_Platform (GameId, PlatformId) VALUES (@GameId, @PlatformId)";
      await db.ExecuteAsync(deterministicInsert, deterministicFakeGamePlatformRelations);

      var randomGamePlatformFaker = new Faker<FakeGamePlatformRelation>()
      .RuleFor(gp => gp.GameId, f => f.PickRandom(gameIds))
      .RuleFor(gp => gp.PlatformId, f => f.PickRandom(platformIds));

      var randomFakeGamePlatformRelations = randomGamePlatformFaker.Generate(SeedDataConfig.FakeGameRelationMultiplier * SeedDataConfig.FakeGameAmount);
      //Note to myself: REPLACE is MySQL-specific and NOT standard SQL, probably has to be changed upon db-switch
      var randomInsert = "REPLACE Game_Platform (GameId, PlatformId) VALUES (@GameId, @PlatformId)";
      await db.ExecuteAsync(randomInsert, randomFakeGamePlatformRelations);

    }

    var gameGenreRelationCount = await GetGameGenreRelationCount();
    if (gameGenreRelationCount == 0)
    {
      var gameIds = (await db.QueryAsync<int>("SELECT Id FROM Game")).ToList();
      var genreIds = (await db.QueryAsync<int>("SELECT Id FROM Genre")).ToList();

      //Each game should have at least one genre to ensure this I'm using two fakers as a workaround.
      //the first one uses deterministic game ids, the second one random ones
      var gameId = 0;
      var deterministicGameGenreFaker = new Faker<FakeGameGenreRelation>()
      .RuleFor(gp => gp.GameId, f => gameIds[gameId++])
      .RuleFor(gp => gp.GenreId, f => f.PickRandom(genreIds));

      var deterministicFakeGameGenreRelations = deterministicGameGenreFaker.Generate(SeedDataConfig.FakeGameAmount);
      var deterministicInsert = "INSERT INTO Game_Genre (GameID, GenreId) VALUES (@GameId, @GenreId)";
      await db.ExecuteAsync(deterministicInsert, deterministicFakeGameGenreRelations);

      var randomGameGenreFaker = new Faker<FakeGameGenreRelation>()
      .RuleFor(gp => gp.GameId, f => f.PickRandom(gameIds))
      .RuleFor(gp => gp.GenreId, f => f.PickRandom(genreIds));

      var randomFakeGameGenreRelations = randomGameGenreFaker.Generate(SeedDataConfig.FakeGameRelationMultiplier * SeedDataConfig.FakeGameAmount);
      //Note to myself: REPLACE is MySQL-specific and NOT standard SQL, probably has to be changed upon db-switch
      var randomInsert = "REPLACE Game_Genre (GameID, GenreId) VALUES (@GameId, @GenreId)";
      await db.ExecuteAsync(randomInsert, randomFakeGameGenreRelations);

    }

  }
}

