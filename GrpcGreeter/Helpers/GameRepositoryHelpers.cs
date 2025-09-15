using System.Data;
using Dapper;
using GrpcGreeter.FakerClasses;
using GrpcGreeter.Models;

namespace GrpcGreeter.Helpers;

public static class GameRepositoryHelpers
{
    
    public static async Task<List<uint>> GetIdsAsync(IDbConnection db,string tableName)
    {
        var ids = (await db.QueryAsync<uint>($"SELECT Id FROM {tableName}")).ToList();
        return ids;
    }
    
    public static async Task<uint> GetCountAsync(IDbConnection db, string tableName)
    {
        var count = await db.ExecuteScalarAsync<uint>($"SELECT COUNT(*) FROM {tableName}");
        return count;
    }
    
    public static async Task<int> InsertGames(IDbConnection db, List<GameInsertData> games)
    {
        var insertGames = @"
            INSERT INTO Game (Name, ReleaseDate, PublisherId, DeveloperId) 
            VALUES (@Name, @ReleaseDate, @PublisherId, @DeveloperId)";
        var response = await db.ExecuteAsync(insertGames, games);
        return response;
    }
    
    public static async Task<int> InsertGameRatings(IDbConnection db, List<GameRatingUpsertData> ratings)
    {
        var insertRatings = @"
            INSERT INTO GameRating (GameId, Ip, Rating) 
            VALUES (@GameId, @Ip, @Rating);";
        var response = await db.ExecuteAsync(insertRatings, ratings);
        return response;
    }
    
    public static async Task<int> InsertGamePlatformRelations(IDbConnection db, List<IGameRelation>  relations)
    {
        var insert = @"
            INSERT INTO Game_Platform (GameId, PlatformId) 
            VALUES (@GameId, @RelatedTableId)";
        var response = await db.ExecuteAsync(insert, relations);
        return response;
    }
    
    public static async Task<int> InsertGameGenreRelations(IDbConnection db, List<IGameRelation>  relations)
    {
        var insert = @"
            INSERT INTO GGame_Genre (GameID, GenreId)
            VALUES (@GameId, @RelatedTableId)";
        var response = await db.ExecuteAsync(insert, relations);
        return response;
    }
}

