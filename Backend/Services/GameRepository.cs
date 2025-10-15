using GrpcGreeter.Helpers;
using Dapper;
using MySql.Data.MySqlClient;
using System.Data;
using Grpcgreeter.Helpers;
using Shared.Models;

namespace GrpcGreeter.Services;

public class GameRepository
{
  //I'm not sure if this is the best exception to throw
  private readonly string _connectionString = 
    Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING")
    ?? throw new InvalidOperationException("Connection string not set in environment variables.");
  
  private IDbConnection Connection => new MySqlConnection(_connectionString);

  public async Task<IEnumerable<GameDto>>GetGamesAndRatingsAsync(string clientIp, Pagination? pagination = null)
  {
    var paginationString = pagination == null ? "" : $"LIMIT {pagination.Offset}, {pagination.Limit}";
 
    var query = @$"
        SELECT
          g.Id AS Id, g.Name AS Name, g.ReleaseDate AS ReleaseDate,
          p.Name AS Publisher,
          d.Name AS Developer,
          GROUP_CONCAT(DISTINCT pl.Name) AS Platform,
          GROUP_CONCAT(DISTINCT gen.Name) AS Genre,
          ur.Rating AS UserRating,
          AVG(r.Rating) AS AverageRating,
          COUNT(DISTINCT c.Id)  AS CommentCount
        FROM Game g
         LEFT JOIN Publisher p ON g.PublisherId = p.Id
         LEFT JOIN Developer d ON g.DeveloperId = d.Id
         LEFT JOIN Game_Platform gm ON g.Id = gm.GameId
         LEFT JOIN Platform pl ON gm.PlatformId = pl.Id
         LEFT JOIN Game_Genre gg ON g.Id = gg.GameId
         LEFT JOIN Genre gen ON gen.ID = gg.GenreId
         LEFT  JOIN GameRating r ON g.Id = r.GameId
         LEFT JOIN GameRating ur ON g.Id = ur.GameId AND ur.Ip = @clientIp
         LEFT  JOIN GameComment c ON g.Id = c.GameId AND c.Deleted
        GROUP BY Id, Name, ReleaseDate, Publisher, Developer, UserRating
        {paginationString}
      ;";
    using var db = Connection;
    var games = await db.QueryAsync<GameDto>(query, new{clientIp});
    return games;
  }
  
  public async Task<GameDto> GetGameAsync(int gameId, string clientIp)
  {
    var query = @"
      SELECT
        g.Id AS Id, g.Name AS Name, g.ReleaseDate AS ReleaseDate,
        p.Name AS Publisher,
        d.Name AS Developer,
        GROUP_CONCAT(DISTINCT pl.Name) AS Platform,
        GROUP_CONCAT(DISTINCT gen.Name) AS Genre,
        ur.Rating AS UserRating,
        AVG(r.Rating) AS AverageRating,
        COUNT(DISTINCT c.Id) AS CommentCount
         FROM Game g
        LEFT JOIN Publisher p ON g.PublisherId = p.Id
        LEFT JOIN Developer d ON g.DeveloperId = d.Id
        LEFT JOIN Game_Platform gm ON g.Id = gm.GameId
        LEFT JOIN Platform pl ON gm.PlatformId = pl.Id
        LEFT JOIN Game_Genre gg ON g.Id = gg.GameId
        LEFT JOIN Genre gen ON gen.ID = gg.GenreId
        LEFT JOIN GameRating r ON g.Id = r.GameId
        LEFT JOIN GameRating ur ON g.Id = ur.GameId AND ur.Ip = @clientIp
        LEFT JOIN GameComment c ON g.Id = c.GameId WHERE c.Deleted = 0
        AND g.Id = @gameId
        GROUP BY Id, Name, ReleaseDate, Publisher, Developer, UserRating;
      ";
    using var db = Connection;
    var result = await db.QueryFirstAsync<GameDto>(query, new { gameId, clientIp });
    //var result = await db.ExecuteScalarAsync<GameDto?>(query,  new { gameId });
    return result;
  }

  public async Task<(int insertedGamesCount, int insertedPlatformRelationsCount, int insertedGenreRelationsCount)> AddGame(GameCreationData gameCreationData)
  {
    
    using var db = Connection;
    
    
    var insertedGamesCount = await GameRepositoryHelpers.InsertGame(db, gameCreationData);

    if (insertedGamesCount == 0)
    {
      throw new Exception("Game was not inserted");
    }
    
    var insertedGameId = await GameRepositoryHelpers.GetInsertedGameId(db, gameCreationData);
    
    var insertedPlatformRelationsCount =  await GameRepositoryHelpers.InsertGamePlatformRelations(db, gameCreationData, insertedGameId);
    var insertedGenreRelationsCount = await GameRepositoryHelpers.InsertGameGenreRelations(db, insertedGameId, gameCreationData.GenreIds);
    
    return (insertedGamesCount, insertedPlatformRelationsCount, insertedGenreRelationsCount);
    
  }

  public async Task<(int updatedGamesCount, (int deletedPlatformRelationsCount, int addedPlatformRelationsCount, int unchangedPlatformRelationsCount), (int addedGenreRelationsCount, int deletedGenreRelationsCount, int unchangedGenreRelationsCount))> UpdateGame(int gameId, GameCreationData gameCreationData)
  {
    using var db = Connection;

    var sql = @"
      UPDATE Game
      SET Name = @Name, ReleaseDate = @ReleaseDate, PublisherId = @PublisherId, DeveloperId = @DeveloperId
      WHERE Id = @gameId
    ";
    
    var updatedGamesCount = await db.ExecuteScalarAsync<int>(sql, new { gameId, gameCreationData.Name, gameCreationData.ReleaseDate, gameCreationData.PublisherId,
      gameCreationData.DeveloperId  } );

    var platformRelationCounts =
      await GameRepositoryHelpers.HandleGamePlatformRelationChanges(db, gameId, gameCreationData.PlatformIds);

    var genreRelationCounts = await GameRepositoryHelpers.HandleGameGenreRelationChanges(db,  gameId, gameCreationData.GenreIds);
    
    return (updatedGamesCount, platformRelationCounts, genreRelationCounts);

  }

  public async Task<(int deletedGamesCount, int deletedGamePlatformRelationsCount, int deletedGameGenreRelationsCount, int deletedCommentsCount, int deletedGameRatingsCount)> DeleteGameAsync(int gameId)
  {
    using var db = Connection;
    var gamePlatformRelationIds = await GameRepositoryHelpers.GetRelatedGameDataIdsAsync(db, TableNames.GamePlatformRelation, gameId);
    var gprIds = gamePlatformRelationIds.ToArray();
    var deletedGamePlatformRelationsCount = await GameRepositoryHelpers.DeleteIdsFromTableAsync(db, TableNames.GamePlatformRelation, gprIds);
    
    var gameGenreRelationIds = await GameRepositoryHelpers.GetRelatedGameDataIdsAsync(db, TableNames.GameGenreRelation, gameId);
    var ggrIds = gameGenreRelationIds.ToArray();
    var deletedGameGenreRelationsCount = await GameRepositoryHelpers.DeleteIdsFromTableAsync(db, TableNames.GameGenreRelation, ggrIds);
    
    var gameCommentIds = await GameRepositoryHelpers.GetRelatedGameDataIdsAsync(db, TableNames.GameComment, gameId);
    var gcIds = gameCommentIds.ToArray();
    var deletedCommentsCount = await GameRepositoryHelpers.DeleteIdsFromTableAsync(db, TableNames.GameComment, gcIds);
    
    var gameRatingIds = await GameRepositoryHelpers.GetRelatedGameDataIdsAsync(db, TableNames.GameRating, gameId);
    var grIds = gameRatingIds.ToArray();
    var deletedGameRatingsCount = await  GameRepositoryHelpers.DeleteIdsFromTableAsync(db, TableNames.GameRating, grIds);
    
    var deletedGamesCount = await GameRepositoryHelpers.DeleteIdsFromTableAsync(db, TableNames.Game, [gameId]);
    
    return (deletedGamesCount, deletedGamePlatformRelationsCount, deletedGameGenreRelationsCount, deletedCommentsCount, deletedGameRatingsCount);
  }
  
  public async Task<IEnumerable<int?>> GetUserRatingsAsync(int[] gameIds, string userIp)
  {
    var sql = @"
      SELECT Rating
      FROM GameRating
      WHERE GameId IN @gameIds AND Ip = @userIp
    ";
    using var db = Connection;
    return await db.QueryAsync<int?>(sql, new { gameIds, userIp });
  }
  
  public async Task<IEnumerable<GameRatingDto>> GetAllUserRatingsAsync(string userIp)
  {
    var sql = @"
      SELECT GameId, Rating
      FROM GameRating
      WHERE Ip = @userIp
    ";
    using var db = Connection;
    return await db.QueryAsync<GameRatingDto>(sql, new { userIp });
  }
  
  
  public async Task<IEnumerable<GameCommentDto>> GetGameCommentsForGameAsync(int gameId, Pagination?  pagination = null)
  {
    var paginationInsert = pagination == null ? "" : $"LIMIT {pagination.Offset}, {pagination.Limit}";

    
    using var db = Connection;
    string sql = @$"
      SELECT Id, GameId, ParentId, Ip, Content, Deleted, Edited, CreatedAt, UpdatedAt
      FROM GameComment
      WHERE GameId = @gameId
      {paginationInsert}
    ;";
    return await db.QueryAsync<GameCommentDto>(sql, new { gameId });
  }


  public async Task<string> GetGameCommentIpAsync(int commentId)
  {
    using var db = Connection;
    string sql = "SELECT Ip FROM GameComment WHERE Id = @Id;";
    return await db.QueryFirstAsync<string>(sql, new { Id = commentId });
  }

  public async Task<int> UpdateGameCommentAsync(GameCommentUpsertData gameComment)
  {
    using var db = Connection;
    string sql = @"
      UPDATE GameComment
      SET Content = @Content, Edited = 1
      WHERE Id = @Id;
    ";
    return await db.ExecuteAsync(sql, gameComment);
  }

  public async Task<int> HardDeleteGameCommentAsync(int commentId)
  {
    using var db = Connection;
    string sql = @"
        DELETE FROM GameComment 
        WHERE Id = @Id;
    ";
    return await db.ExecuteAsync(sql, new { Id = commentId });
  }

  public async Task<int> SoftDeleteGameCommentAsync(int commentId)
  {
    using var db = Connection;
    string sql = @"
      UPDATE GameComment 
      SET Deleted = 1, Content = ''
      WHERE Id = @Id;
    ";
    return await db.ExecuteAsync(sql, new { Id = commentId });
    
    
  }
  
  public async Task<bool> GameToInsertExistsAsync(GameCreationData gameCreationData)
  {
    using var db = Connection;
    
    var sql = @"
      SELECT COUNT(*)
      FROM Game
      WHERE 
        Name = @Name AND 
        ReleaseDate = @ReleaseDate AND 
        PublisherId = @PublisherId AND 
        DeveloperId = @DeveloperId;
    ";
    
    return await db.ExecuteScalarAsync<int>(sql, gameCreationData) > 0;
            
            
  }

  public async Task<bool> IdExistsInTableAsync(int gameId, string tableName)
  {
    using var db = Connection;
    string sql = $"SELECT COUNT(*) FROM {tableName} WHERE Id = @Id;";
    return await db.ExecuteScalarAsync<int>(sql, new { Id = gameId }) > 0;
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
  
  public async Task<bool> GameCommentExistsAsync(int? commentId)
  {
    if (commentId == null)
    {
      return false;
    }
    
    using var db = Connection;
    var sql = "SELECT COUNT(*) FROM GameComment WHERE Id = @Id";
    var result = await db.ExecuteScalarAsync<int>(sql, new { Id = commentId });
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
  
  public async Task<int> AddGameCommentAsync(GameCommentUpsertData comment)
  {
    using var db = Connection;
    var sql = @"
      INSERT INTO GameComment (GameId, ParentId, Ip, Content )
      VALUES (@GameId, @ParentId, @Ip, @Content)";

    var response = await db.ExecuteAsync(sql, comment);
    return response;
  }

  public async Task<int> UpdateRatingAsync(GameRatingUpsertData rating)
  {
    using var db = Connection;
    var sql = @"
      UPDATE GameRating 
      SET Rating = @Rating
      WHERE Ip = @Ip AND GameId = @GameId";
    var response = await db.ExecuteAsync(sql, rating);
    return response;

  }

  public async Task<IEnumerable<NameIdDto>>GetAllGenresAsync()
  {
    using var db = Connection;
    var sql = @"
      SELECT Id, Name
      FROM Genre;
    ";
    return await db.QueryAsync<NameIdDto>(sql);
  }
  
  public async Task<IEnumerable<NameIdDto>>GetAllPlatformsAsync()
  {
    using var db = Connection;
    var sql = @"
      SELECT Id, Name
      FROM Platform;
    ";
    return await db.QueryAsync<NameIdDto>(sql);
  }
  
  public async Task<IEnumerable<NameIdDto>>GetAllPublishersAsync()
  {
    using var db = Connection;
    var sql = @"
      SELECT Id, Name
      FROM Publisher;
    ";
    return await db.QueryAsync<NameIdDto>(sql);
  }
  
  public async Task<IEnumerable<NameIdDto>>GetAllDevelopersAsync()
  {
    using var db = Connection;
    var sql = @"
      SELECT Id, Name
      FROM Developer;
    ";
    return await db.QueryAsync<NameIdDto>(sql);
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
    
    var commentCount = await GameRepositoryHelpers.GetCountAsync(db,"GameComment");
    if (commentCount == 0)
    { 
      var seededGameCommentsAmount = await DbSeeder.SeedGameComments(db);
      Console.WriteLine("Seeding database with {0} comments.", seededGameCommentsAmount);
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

