using Grpcgreeter.Helpers;
using GrpcGreeter.Models;


namespace GrpcGreeter.Helpers;

using Bogus;

public static class FakerGenerator
{
    //there are some redundant Func constructors in here, because sometimes the IDE somtimes likes to display
    //errors that aren't really errors when the RuleFor method is used
    public static Faker<GameInsertData> GameFaker(List<uint> publisherIds, List<uint> developerIds)
    {
        return new Faker<GameInsertData>()
            .RuleFor(g => g.Name, f => f.Commerce.ProductName())
            .RuleFor(g => g.ReleaseDate, new Func<Faker, uint>(f => 
                (uint)f.Random.Int(SeedDataConfig.MinReleaseDate, SeedDataConfig.MaxReleaseDate)))
            .RuleFor(g => g.PublisherId, new Func<Faker, uint>(f => f.PickRandom(publisherIds)))
            .RuleFor(g => g.DeveloperId, new Func<Faker, uint>(f => f.PickRandom(developerIds)));
    }

    public static Faker<GameRatingUpsertData> GameRatingUpsertFaker(List<uint> gameIds)
    {
        return new Faker<GameRatingUpsertData>()
            .RuleFor(r => r.GameId, (new Func<Faker, int>(f => (int)f.PickRandom(gameIds))))
            .RuleFor(r => r.Ip, f => f.Internet.Ip())
            .RuleFor(r => r.Rating, (f => (int)f.Random.Int(SeedDataConfig.MinRating, SeedDataConfig.MaxRating)));
    }
    
    public static Faker<GameRelation> SemiDeterministicGameRelationFaker(List<uint> gameIds, List<uint> relatedTableIds)
    {
        Console.WriteLine($"game ids length: {gameIds.Count}");
        var gameId = 0;
        return new Faker<GameRelation>()
            .RuleFor(gp => gp.GameId, f => gameIds[gameId++])
            .RuleFor(gp => gp.RelatedTableId, f => f.PickRandom(relatedTableIds));
    }
    
    public static Faker<GameRelation> RandomPlatformRelationFaker(List<uint> gameIds, List<uint> relatedTableIds)
    {
        return new Faker<GameRelation>()
            .RuleFor(gp => gp.GameId, f => f.PickRandom(gameIds))
            .RuleFor(gp => gp.RelatedTableId, f => f.PickRandom(relatedTableIds));
    }
}
