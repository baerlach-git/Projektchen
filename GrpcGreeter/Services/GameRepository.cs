namespace GrpcGreeter.Services;

using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using Bogus;
using GrpcGreeter.Models;
using GrpcGreeter.FakerClasses;





public class GameRepository
{
  private readonly string _connectionString;

  public GameRepository()
  {
    _connectionString = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING")
                        ?? throw new InvalidOperationException("Connection string not set in environment variables.");
  }

  
  private IDbConnection Connection => new MySqlConnection(_connectionString);

  public async Task<IEnumerable<Game>> GetGamesWithRatingsAsync()
  {
    var query = @"
      SELECT 
        g.Id AS Id, g.Name AS Name, g.ReleaseDate AS ReleaseDate,
        p.Name AS Publisher, 
        d.Name AS DevStudio, 
        GROUP_CONCAT(DISTINCT pl.Name) AS Platform,
        GROUP_CONCAT(DISTINCT gen.Name) AS Genre,
        AVG(r.Rating) AS AverageRating
      FROM Game g
      JOIN Publisher p ON g.PublisherId = p.Id
      JOIN DevStudio d ON g.DevStudioId = d.Id
      JOIN Game_Platform gm ON g.Id = gm.GameId
      JOIN Platform pl ON gm.PlatformId = pl.Id
      JOIN Game_Genre gg ON g.Id = gg.GameId
      JOIN Genre gen ON gen.ID = gg.GenreId
      JOIN GameRating r ON g.Id = r.GameId
      GROUP BY Id, Name, ReleaseDate, Publisher, DevStudio;
      ";
    using var db = Connection;

    var result = await db.QueryAsync<Game>(query);
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

  public async Task AddRatingAsync(GrpcGameService.GameRating rating)
  {
    using var db = Connection;
    var sql = @"
      INSERT INTO GameRating (GameId, Ip, Rating)
      VALUES (@GameId, @Ip, @Rating)";

    await db.ExecuteAsync(sql, rating);
  }

  public async Task UpdateRatingAsync(GameRating rating)
  {
    using var db = Connection;
    var sql = @"
      UPDATE GameRating 
      SET Rating = @Rating
      WHERE Id = @Id";
    await db.ExecuteAsync(sql, rating);

  }


  public async Task SeedDatabaseAsync()
  {
    using var db = Connection;

    // Seed data if table is empty
    var fakeGameAmount = 100;

    var gameCount = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Game");
    if (gameCount == 0)
    {
      var publisherIds = (await db.QueryAsync<int>("SELECT Id FROM Publisher")).ToList();
      var genreIds = (await db.QueryAsync<int>("SELECT Id FROM Genre")).ToList();
      var platformIds = (await db.QueryAsync<int>("SELECT Id FROM Platform")).ToList();
      var DevStudioIds = (await db.QueryAsync<int>("SELECT Id FROM DevStudio")).ToList();





      var gameFaker = new Faker<FakeGame>()
        .RuleFor(g => g.Name, f => f.Commerce.ProductName())
        .RuleFor(g => g.ReleaseDate, f => f.Random.Int(1980, 2025))
        .RuleFor(g => g.PublisherId, (f, g) => f.PickRandom(publisherIds))
        .RuleFor(g => g.DevStudioId, (f, g) => f.PickRandom(DevStudioIds));

      var fakeGames = gameFaker.Generate(fakeGameAmount);

      var insertGames = "INSERT INTO Game (Name, ReleaseDate, PublisherId, DevStudioId) VALUES (@Name, @ReleaseDate, @PublisherId, @DevStudioId)";
      await db.ExecuteAsync(insertGames, fakeGames);
    }


    var ratingsCount = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM GameRating");
    if (ratingsCount == 0)
    {
      var gameIds = (await db.QueryAsync<int>("SELECT Id FROM Game")).ToList();

      var ratingFaker = new Faker<GameRating>()
        .RuleFor(r => r.GameId, f => f.PickRandom(gameIds))
        .RuleFor(r => r.Ip, f => f.Internet.Ip())
        .RuleFor(r => r.Rating, f => f.Random.Int(1, 5));

      var ratings = ratingFaker.Generate(1000);
      var insertRatings = @"
      INSERT INTO GameRating (GameId, Ip, Rating) VALUES (@GameId, @Ip, @Rating);";
      await db.ExecuteAsync(insertRatings, ratings);
    }

    var gamePlatformCount = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Game_Platform");
    if (gamePlatformCount == 0)
    {
      var gameIds = (await db.QueryAsync<int>("SELECT Id FROM Game")).ToList();
      var platformIds = (await db.QueryAsync<int>("SELECT Id FROM Platform")).ToList();

      //Each game should have at least one platform to ensure this I'm using two fakers as a workaround.
      //the first one uses deterministic game ids, the second one random ones
      var gameId = 0;
      var deterministicGamePlatformFaker = new Faker<FakeGamePlatformRelation>()
      .RuleFor(gp => gp.GameId, f => gameIds[gameId++])
      .RuleFor(gp => gp.PlatformId, f => f.PickRandom(platformIds));

      var deterministicFakeGamePlatformRelations = deterministicGamePlatformFaker.Generate(fakeGameAmount);
      var deterministicInsert = "INSERT INTO Game_Platform (GameId, PlatformId) VALUES (@GameId, @PlatformId)";
      await db.ExecuteAsync(deterministicInsert, deterministicFakeGamePlatformRelations);

      var randomGamePlatformFaker = new Faker<FakeGamePlatformRelation>()
      .RuleFor(gp => gp.GameId, f => f.PickRandom(gameIds))
      .RuleFor(gp => gp.PlatformId, f => f.PickRandom(platformIds));

      var randomFakeGamePlatformRelations = randomGamePlatformFaker.Generate(4 * fakeGameAmount);
      //Note to myself: REPLACE is MySQL-specific and NOT standard SQL, probably has to be changed upon db-switch
      var randomInsert = "REPLACE Game_Platform (GameId, PlatformId) VALUES (@GameId, @PlatformId)";
      await db.ExecuteAsync(randomInsert, randomFakeGamePlatformRelations);

    }

    var gameGenreCount = await db.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Game_Genre");
    if (gameGenreCount == 0)
    {
      var gameIds = (await db.QueryAsync<int>("SELECT Id FROM Game")).ToList();
      var genreIds = (await db.QueryAsync<int>("SELECT Id FROM Genre")).ToList();

      //Each game should have at least one genre to ensure this I'm using two fakers as a workaround.
      //the first one uses deterministic game ids, the second one random ones
      var gameId = 0;
      var deterministicGameGenreFaker = new Faker<FakeGameGenreRelation>()
      .RuleFor(gp => gp.GameId, f => gameIds[gameId++])
      .RuleFor(gp => gp.GenreId, f => f.PickRandom(genreIds));

      var deterministicFakeGameGenreRelations = deterministicGameGenreFaker.Generate(fakeGameAmount);
      var deterministicInsert = "INSERT INTO Game_Genre (GameID, GenreId) VALUES (@GameId, @GenreId)";
      await db.ExecuteAsync(deterministicInsert, deterministicFakeGameGenreRelations);

      var randomGameGenreFaker = new Faker<FakeGameGenreRelation>()
      .RuleFor(gp => gp.GameId, f => f.PickRandom(gameIds))
      .RuleFor(gp => gp.GenreId, f => f.PickRandom(genreIds));

      var randomFakeGameGenreRelations = randomGameGenreFaker.Generate(4 * fakeGameAmount);
      //Note to myself: REPLACE is MySQL-specific and NOT standard SQL, probably has to be changed upon db-switch
      var randomInsert = "REPLACE Game_Genre (GameID, GenreId) VALUES (@GameId, @GenreId)";
      await db.ExecuteAsync(randomInsert, randomFakeGameGenreRelations);

    }

  }
}

