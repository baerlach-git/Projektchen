using GrpcGreeter.Helpers;

namespace GrpcGreeter.Services;

using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using Models;
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

  public async Task<bool> GameExistsAsync(int gameId)
  {
    using var db = Connection;
    var sql = "SELECT COUNT(*) FROM Game WHERE Id = @Id";
    var result = await db.ExecuteScalarAsync<int>(sql, new { Id = gameId });
    return result > 0;
  }

  public async Task<bool> RatingExistsAsync(int gameId, string Ip)
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

    var response = await db.ExecuteAsync(sql, rating);
    return response;
  }

  public async Task<int> UpdateRatingAsync(GameRatingUpsertData rating)
  {
    using var db = Connection;
    var sql = @"
      UPDATE GameRatingg 
      SET Rating = @Rating
      WHERE Ip = @Id";
    var response = await db.ExecuteAsync(sql, rating);
    return response;

  }

  public async Task SeedDatabaseAsync()
  {
    using var db = Connection;
    //Maybe throw exceptions here if the seeded amounts do not match the expectation according to teh config?
    var gameCount = await GameRepositoryHelpers.GetCountAsync(db,"Game");
    if (gameCount == 0)
    {
      var seededGameAmount = await DbSeeder.SeedGames(db);
      Console.WriteLine("Seeding database with {0} games.", seededGameAmount);
    }


    var ratingCount = await GameRepositoryHelpers.GetCountAsync(db,"GameRating");
    if (ratingCount == 0)
    {
      var seededGameRatingsAmount = await DbSeeder.SeedGameRatings(db);
      Console.WriteLine("Seeding database with {0} game ratings.", seededGameRatingsAmount);

    }

    var gamePlatformRelationCount = await GameRepositoryHelpers.GetCountAsync(db, "Game_Platform");
    if (gamePlatformRelationCount == 0)
    {
      var seededGamePlatformRelationsAmount = await DbSeeder.SeedGamePlatformRelations(db);
      Console.WriteLine(
        "Seeding database with game platform relations: {0} deterministic relations, {1} random relations", 
        seededGamePlatformRelationsAmount.deterministicInsertResponse,  seededGamePlatformRelationsAmount.randomInsertResponse);

    }

    var gameGenreRelationCount = await GameRepositoryHelpers.GetCountAsync(db, "Game_Genre");
    if (gameGenreRelationCount == 0)
    {
      var seededGameGenreRelationsAmount = await DbSeeder.SeedGameGenreRelations(db);
      Console.WriteLine("" +
        "Seeding database with game genre relations: {0} deterministic relations, {1} random relations", 
        seededGameGenreRelationsAmount.deterministicInsertResponse,  seededGameGenreRelationsAmount.randomInsertResponse);
    }

  }
}

