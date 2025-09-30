namespace Grpcgreeter.Helpers;

public static class SeedDataConfig
{
    public const int FakeGameAmount = 100;
    public const int FakeGameRelationMultiplier = 4;
    public const int MinReleaseDate = 1970; // must at least be 1950 enforced by database
    public const int MaxReleaseDate = 2025;
    public const int MinRating = 1; // must at least be 1 enforced by database
    public const int MaxRating = 5; // must at max be 5 enforced by database
    public const int FakeRatingAmount = 1000;
    public const int FakeCommentAmount = 1000;
    public const int MaxCommentLength = 1000;

}