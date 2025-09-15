using System.Data;
using GrpcGreeter.Helpers;
using GrpcGreeter.Models;

namespace Grpcgreeter.Helpers;


public static class DbSeeder
{
    public static async Task<int> SeedGames(IDbConnection db)
    {
        var publisherIds = await GameRepositoryHelpers.GetIdsAsync(db, "Publisher");
        var developerIds = await GameRepositoryHelpers.GetIdsAsync(db, "Developer");
        var gameFaker = FakerGenerator.GameFaker(publisherIds, developerIds);
        var fakeGames = gameFaker.Generate(SeedDataConfig.FakeGameAmount);
        //TODO check for return value and throw an exception if it 0 even though FakeGameAmount is not 0
        var response = await GameRepositoryHelpers.InsertGames(db, fakeGames);
        return response;
    }

    public static async Task<int> SeedGameRatings(IDbConnection db)
    {
        var gameIds = await GameRepositoryHelpers.GetIdsAsync(db,"Game");
        var ratingFaker = FakerGenerator.GameRatingUpsertFaker(gameIds);
        var ratings = ratingFaker.Generate(SeedDataConfig.FakeRatingAmount);
        var response = await GameRepositoryHelpers.InsertGameRatings(db, ratings);
        return response;
    }

    public static async Task<(int deterministicInsertResponse, int randomInsertResponse)> SeedGamePlatformRelations(
        IDbConnection db)
    {
        var gameIds = await GameRepositoryHelpers.GetIdsAsync(db,"Game");
        var platformIds = await GameRepositoryHelpers.GetIdsAsync(db,"Platform");

        //Each game should have at least one platform to ensure this I'm using two fakers as a workaround.
        //the first one uses deterministic game ids, the second one random ones
        var deterministicGamePlatformRelationFaker = FakerGenerator.SemiDeterministicGameRelationFaker(gameIds, platformIds);
        var deterministicFakeGamePlatformRelations = 
                deterministicGamePlatformRelationFaker.Generate(SeedDataConfig.FakeGameAmount);
        var deterministicInsertResponse = 
            await GameRepositoryHelpers.InsertGamePlatformRelations(db, deterministicFakeGamePlatformRelations);

        var randomGamePlatformFaker = FakerGenerator.RandomPlatformRelationFaker(gameIds, platformIds);
        var randomFakeGamePlatformRelations = 
            randomGamePlatformFaker.Generate(SeedDataConfig.FakeGameRelationMultiplier * SeedDataConfig.FakeGameAmount);
        var randomInsertResponse = 
            await GameRepositoryHelpers.InsertGamePlatformRelations(db, randomFakeGamePlatformRelations);
        return (deterministicInsertResponse, randomInsertResponse);
    }

    public static async Task<(int deterministicInsertResponse, int randomInsertResponse)> SeedGameGenreRelations(
        IDbConnection db)
    {
        var gameIds = await GameRepositoryHelpers.GetIdsAsync(db, "Game");
        var genreIds = await GameRepositoryHelpers.GetIdsAsync(db, "Genre");

        //Each game should have at least one genre to ensure this I'm using two fakers as a workaround.
        //the first one uses deterministic game ids, the second one random ones
        var deterministicGameGenreFaker = FakerGenerator.SemiDeterministicGameRelationFaker(gameIds, genreIds);
        var deterministicFakeGameGenreRelations = deterministicGameGenreFaker.Generate(SeedDataConfig.FakeGameAmount);
        var deterministicInsertResponse = await GameRepositoryHelpers.InsertGameGenreRelations(db, deterministicFakeGameGenreRelations);

        var randomGameGenreFaker = FakerGenerator.RandomPlatformRelationFaker(gameIds, genreIds);
        var randomFakeGameGenreRelations = randomGameGenreFaker.Generate(SeedDataConfig.FakeGameRelationMultiplier * SeedDataConfig.FakeGameAmount);
        var  randomInsertResponse = await GameRepositoryHelpers.InsertGameGenreRelations(db, randomFakeGameGenreRelations);
        return (deterministicInsertResponse, randomInsertResponse);
    }
    
    
}