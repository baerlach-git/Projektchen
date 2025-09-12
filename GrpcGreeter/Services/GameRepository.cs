using GrpcGreeter.Helpers;

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
  
  private async Task<int> GetCountAsync(string tableName)
  {
    using var db = Connection;
    var count = await db.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {tableName}");
    return count;
  }
  
  private async Task<List<int>> GetIdsAsync(string tableName)
  {
    using var db = Connection;
    var ids = (await db.QueryAsync<int>($"SELECT Id FROM {tableName}")).ToList();
    return ids;
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
    
    var gameCount = await GetCountAsync("Game");
    if (gameCount == 0)
    {
      var publisherIds = await GetIdsAsync("Publisher");
      var developerIds = await GetIdsAsync("Developer");
      var gameFaker = FakerGenerator.GameFaker(publisherIds, developerIds);
      var fakeGames = gameFaker.Generate(SeedDataConfig.FakeGameAmount);
      //TODO check for return value and throw an exception if it 0 even though FakeGameAmount is not 0
      await InsertGames(fakeGames);
    }


    var ratingCount = await GetCountAsync("GameRating");
    if (ratingCount == 0)
    {
      var gameIds = await GetIdsAsync("Game");
      var ratingFaker = FakerGenerator.GameRatingUpsertFaker(gameIds);

      var ratings = ratingFaker.Generate(SeedDataConfig.FakeRatingAmount);
      await InsertGameRatings(ratings);
    }

    var gamePlatformRelationCount = await GetCountAsync("Game_Platform");
    if (gamePlatformRelationCount == 0)
    {
      var gameIds = await GetIdsAsync("Game");
      var platformIds = await GetIdsAsync("Platform");

      //Each game should have at least one platform to ensure this I'm using two fakers as a workaround.
      //the first one uses deterministic game ids, the second one random ones
      var deterministicGamePlatformRelationFaker = FakerGenerator.SemiDeterministicGameRelationFaker(gameIds, platformIds);
      var deterministicFakeGamePlatformRelations = deterministicGamePlatformRelationFaker.Generate(SeedDataConfig.FakeGameAmount);
      var deterministicInsert = "INSERT INTO Game_Platform (GameId, PlatformId) VALUES (@GameId, @PlatformId)";
      await db.ExecuteAsync(deterministicInsert, deterministicFakeGamePlatformRelations);

      var randomGamePlatformFaker = FakerGenerator.RandomPlatformRelationFaker(gameIds, platformIds);
      var randomFakeGamePlatformRelations = randomGamePlatformFaker.Generate(SeedDataConfig.FakeGameRelationMultiplier * SeedDataConfig.FakeGameAmount);
      //Note to myself: REPLACE is MySQL-specific and NOT standard SQL, probably has to be changed upon db-switch
      var randomInsert = "REPLACE Game_Platform (GameId, PlatformId) VALUES (@GameId, @PlatformId)";
      await db.ExecuteAsync(randomInsert, randomFakeGamePlatformRelations);

    }

    var gameGenreRelationCount = await GetCountAsync("Game_Genre");
    if (gameGenreRelationCount == 0)
    {
      var gameIds = await GetIdsAsync("Game");
      var genreIds = await GetIdsAsync("Genre");

      //Each game should have at least one genre to ensure this I'm using two fakers as a workaround.
      //the first one uses deterministic game ids, the second one random ones
      var gameId = 0;
      var deterministicGameGenreFaker = FakerGenerator.SemiDeterministicGameRelationFaker(gameIds, genreIds);

      var deterministicFakeGameGenreRelations = deterministicGameGenreFaker.Generate(SeedDataConfig.FakeGameAmount);
      var deterministicInsert = "INSERT INTO Game_Genre (GameID, GenreId) VALUES (@GameId, @GenreId)";
      await db.ExecuteAsync(deterministicInsert, deterministicFakeGameGenreRelations);

      var randomGameGenreFaker = FakerGenerator.RandomPlatformRelationFaker(gameIds, genreIds);
      var randomFakeGameGenreRelations = randomGameGenreFaker.Generate(SeedDataConfig.FakeGameRelationMultiplier * SeedDataConfig.FakeGameAmount);
      //Note to myself: REPLACE is MySQL-specific and NOT standard SQL, probably has to be changed upon db-switch
      var randomInsert = "REPLACE Game_Genre (GameID, GenreId) VALUES (@GameId, @GenreId)";
      await db.ExecuteAsync(randomInsert, randomFakeGameGenreRelations);

    }

  }
}

