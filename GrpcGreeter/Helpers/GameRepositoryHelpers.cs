using System.Data;
using Dapper;
using Shared.Models;

namespace GrpcGreeter.Helpers;

public static class GameRepositoryHelpers
{
    
    public static async Task<List<int>> GetIdsAsync(IDbConnection db,string tableName)
    {
        var ids = (await db.QueryAsync<int>($"SELECT Id FROM {tableName}")).ToList();
        return ids;
    }
    
    public static async Task<int> GetCountAsync(IDbConnection db, string tableName)
    {
        var count = await db.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {tableName}");
        return count;
    }
    
    public static async Task<int> InsertGames(IDbConnection db, List<GameInsertData> games)
    {
        Console.WriteLine($"ConnectionString {db.ConnectionString}");
        var insertGames = @"
            INSERT INTO Game (Name, ReleaseDate, PublisherId, DeveloperId) 
            VALUES (@Name, @ReleaseDate, @PublisherId, @DeveloperId)";
        var response = await db.ExecuteAsync(insertGames, games);
        return response;
    }

    public static async Task<int> InsertGame(IDbConnection db, GameCreationData gameCreationData)
    {
        var sql = @"
          INSERT INTO Game
            (Name, ReleaseDate, PublisherId, DeveloperId)
          VALUES
            (@Name, @ReleaseDate, @PublisherId, @DeveloperId);
        ";
    
        return await db.ExecuteAsync(sql, gameCreationData);
    }

    public static async Task<int> GetInsertedGameId(IDbConnection db, GameCreationData gameCreationData)
    {
        var sql = @"
        SELECT Id
        FROM Game
        WHERE 
            Name = @Name AND 
            ReleaseDate = @ReleaseDate AND 
            PublisherId = @PublisherId AND 
            DeveloperId = @DeveloperId;
      ";
      
        return await db.QuerySingleAsync<int>(sql, gameCreationData);
    }

    public static async Task<int> InsertGamePlatformRelations(IDbConnection db, GameCreationData gameCreationData,
        int gameId)
    {
        var sql = @"
        INSERT INTO Game_Platform
            (GameId, PlatformId)
        VALUES 
            (@GameId, @PlatformId);
      ";
      
        var platformInsertionData = gameCreationData.PlatformIds.Select(platformId => new {GameId = gameId, PlatformId = platformId}).ToList();
        return await db.ExecuteAsync(sql, platformInsertionData);
    }
    
    public static async Task<int> InsertGamePlatformRelations(IDbConnection db, int gameId, IEnumerable<int> platformIds)
    {
        var sql = @"
        INSERT INTO Game_Platform
            (GameId, PlatformId)
        VALUES 
            (@GameId, @PlatformId);
      ";
      
        var platformInsertionData = platformIds.Select(platformId => new {GameId = gameId, PlatformId = platformId}).ToList();
        return await db.ExecuteAsync(sql, platformInsertionData);
    }

    public static async Task<int> InsertGameGenreRelations(IDbConnection db, GameCreationData gameCreationData,
        int gameId)
    {
        var sql = @"
        INSERT INTO  Game_Genre
            (GameId, GenreId)
        VALUES
            (@GameId, @GenreId);
      ";
      
        var genreInsertionData = gameCreationData.GenreIds.Select(genreId => new {GameId = gameId, GenreId = genreId}).ToList();
        return await db.ExecuteAsync(sql, genreInsertionData);
    }
    
    public static async Task<int> InsertGameGenreRelations(IDbConnection db, int gameId, IEnumerable<int> genreIds)
    {
        var sql = @"
        INSERT INTO  Game_Genre
            (GameId, GenreId)
        VALUES
            (@GameId, @GenreId);
      ";
      
        var genreInsertionData = genreIds.Select(genreId => new {GameId = gameId, GenreId = genreId}).ToList();
        return await db.ExecuteAsync(sql, genreInsertionData);
    }
    
    public static async Task<int> InsertGameRatings(IDbConnection db, List<GameRatingUpsertData> ratings)
    {
        var insertRatings = @"
            INSERT INTO GameRating (GameId, Ip, Rating) 
            VALUES (@GameId, @Ip, @Rating);";
        var response = await db.ExecuteAsync(insertRatings, ratings);
        return response;
    }

    public static async Task<int> InsertGameComments(IDbConnection db, List<GameCommentUpsertData> comments)
    {
        var insertComments = @"
            INSERT INTO GameComment (GameId, Ip, Content, Deleted)
            VALUES (@GameId, @Ip, @Content, @Deleted);";
        var response = await db.ExecuteAsync(insertComments, comments);
        return response;
    }
    
    public static async Task<int> InsertGamePlatformRelations(IDbConnection db, List<GameRelation>  relations)
    {
        //Replace is MySql specific
        //this may now result in duplicates, since GameId, PlatformId/GenreId are no longer primary keys of these tables
        var insert = @"
            REPLACE Game_Platform (GameId, PlatformId) 
            VALUES (@GameId, @RelatedTableId)";
        var response = await db.ExecuteAsync(insert, relations);
        return response;
    }
    
    public static async Task<int> InsertGameGenreRelations(IDbConnection db, List<GameRelation>  relations)
    {
        var insert = @"
            REPLACE Game_Genre (GameID, GenreId)
            VALUES (@GameId, @RelatedTableId)";
        var response = await db.ExecuteAsync(insert, relations);
        return response;
    }

    public static async Task<IEnumerable<int>> GetAllIdsFromTableAsync(IDbConnection db, string tableName)
    {
        var sql = $"SELECT Id FROM {tableName}";
        return await db.QueryAsync<int>(sql);
    }
    
    public static async Task<IEnumerable<int>> GetRelatedGameDataIdsAsync(IDbConnection db, string tableName, int  gameId)
    {
        var sql = $@"
            SELECT Id 
            FROM {tableName}
            WHERE GameId = @gameId;
        ";
        return await db.QueryAsync<int>(sql, new { gameId });
    }

    public static async Task<int> DeleteIdsFromTableAsync(IDbConnection db, string tableName, int[] ids)
    {
        var sql = $"DELETE FROM {tableName} WHERE Id IN @Ids";
        return await db.ExecuteAsync(sql, new { Ids = ids });
         
    }

    private static CategorizedIds CategorizeIds(IEnumerable<int> updatedIds, IEnumerable<int> existingIds)
    {
        var categorizedPlatformIds = new CategorizedIds();
        
        var updatedIdList = updatedIds.ToList();
        var existingIdList = existingIds.ToList();


        foreach (var id in updatedIdList)
        {
            if (existingIdList.Contains(id))
            {
                categorizedPlatformIds.unchanged.Add(id);
            }
            else
            {
                categorizedPlatformIds.added.Add(id);
            }
        }

        foreach (var id in existingIdList)
        {
            if (!updatedIdList.Contains(id))
            {
                categorizedPlatformIds.deleted.Add(id);
            }
        }
        return  categorizedPlatformIds;
    }

    public static async Task<(int deletedPlatformRelations, int addedPlatformRelations, int unchangedPlatformRelations)>HandleGamePlatformRelationChanges(IDbConnection db, int gameId, IEnumerable<int> platformIds)
    {
        var sql = "SELECT PlatformId FROM Game_Platform WHERE GameId = @gameId";
        var relatedPlatformIds = await db.QueryAsync<int>(sql, new { gameId });
        var categorizedPlatformIds = CategorizeIds(platformIds, relatedPlatformIds.ToArray());

        var deletedPlatformRelations = 0;
        var addedPlatformRelations = 0;
        var unchangedPlatformRelations = categorizedPlatformIds.unchanged.Count;

        if (categorizedPlatformIds.deleted.Count > 0)
        {
            var deleteSql = "DELETE FROM Game_Platform WHERE GameId = @GameId AND PlatformId = @PlatformId";
            var deleteData = categorizedPlatformIds.deleted.Select(pId => new{GameId = gameId, PlatformId = pId});
            deletedPlatformRelations = await db.ExecuteAsync(deleteSql, deleteData);
        }

        if (categorizedPlatformIds.added.Count > 0)
        {
            var insertSql = "INSERT INTO Game_Platform (GameId, PlatformId) VALUES (@GameId, @PlatformId)";
            var insertData = categorizedPlatformIds.added.Select(pId => new{GameId = gameId, PlatformId = pId});
            addedPlatformRelations = await db.ExecuteAsync(insertSql, insertData);
        }

        return (deletedPlatformRelations, addedPlatformRelations, unchangedPlatformRelations);
    }
    
    public static async Task<(int deletedGenreRelations, int addedGenreRelations, int unchangedGenreRelations)>HandleGameGenreRelationChanges(IDbConnection db, int gameId, IEnumerable<int> genreIds)
    {
        var sql = "SELECT GenreId FROM Game_Genre WHERE GameId = @gameId";
        var relatedGenreIds = await db.QueryAsync<int>(sql, new { gameId });
        var categorizedGenreIds = CategorizeIds(genreIds, relatedGenreIds.ToArray());

        var deletedGenreRelations = 0;
        var addedGenreRelations = 0;
        var unchangedGenreRelations = categorizedGenreIds.unchanged.Count;

        if (categorizedGenreIds.deleted.Count > 0)
        {
            var deleteSql = "DELETE FROM Game_Genre WHERE GameId = @GameId AND GenreId = @GenreId";
            var deleteData = categorizedGenreIds.deleted.Select(pId => new{GameId = gameId, GenreId = pId});
            deletedGenreRelations = await db.ExecuteAsync(deleteSql, deleteData);
        }

        if (categorizedGenreIds.added.Count > 0)
        {
            var insertSql = "INSERT INTO Game_Genre (GameId, GenreId) VALUES (@GameId, @GenreId)";
            var insertData = categorizedGenreIds.added.Select(pId => new{GameId = gameId, GenreId = pId});
            addedGenreRelations = await db.ExecuteAsync(insertSql, insertData);
        }

        return (deletedGenreRelations, addedGenreRelations, unchangedGenreRelations);
    }
    
}

