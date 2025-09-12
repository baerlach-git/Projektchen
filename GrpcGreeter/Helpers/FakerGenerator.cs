using GrpcGreeter.FakerClasses;
using Grpcgreeter.Helpers;
using GrpcGreeter.Models;


namespace GrpcGreeter.Helpers;

using Bogus;

public static class FakerGenerator
{
    public static Faker<GameInsertData> GameFaker(List<int> publisherIds, List<int> developerIds)
    {
        return new Faker<GameInsertData>()
            .RuleFor(g => g.Name, f => f.Commerce.ProductName())
            .RuleFor(g => g.ReleaseDate, f => 
                f.Random.Int(SeedDataConfig.MinReleaseDate, SeedDataConfig.MaxReleaseDate))
            .RuleFor(g => g.PublisherId, (f, g) => f.PickRandom(publisherIds))
            .RuleFor(g => g.DeveloperId, (f, g) => f.PickRandom(developerIds));
    }

    public static Faker<GameRatingUpsertData> GameRatingUpsertFaker(List<int> gameIds)
    {
        return new Faker<GameRatingUpsertData>()
            .RuleFor(r => r.GameId, (f => f.PickRandom(gameIds)))
            .RuleFor(r => r.Ip, f => f.Internet.Ip())
            .RuleFor(r => r.Rating, (f => (uint)f.Random.Int(SeedDataConfig.MinRating, SeedDataConfig.MaxRating)));
    }
    
    public static Faker<FakeGamePlatformRelation> SemiDeterministicGameRelationFaker(List<int> gameIds, List<int> relatedTableIds)
    {
        var gameId = 0;
        return new Faker<FakeGamePlatformRelation>()
            .RuleFor(gp => gp.GameId, f => gameIds[gameId++])
            .RuleFor(gp => gp.PlatformId, f => f.PickRandom(relatedTableIds));
    }
    
    public static Faker<FakeGamePlatformRelation> RandomPlatformRelationFaker(List<int> gameIds, List<int> relatedTableIds)
    {
        return new Faker<FakeGamePlatformRelation>()
            .RuleFor(gp => gp.GameId, f => f.PickRandom(gameIds))
            .RuleFor(gp => gp.PlatformId, f => f.PickRandom(relatedTableIds));
    }
}
