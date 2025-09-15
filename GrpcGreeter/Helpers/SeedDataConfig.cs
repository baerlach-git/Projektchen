namespace Grpcgreeter.Helpers;

public static class SeedDataConfig
{
    public const ushort FakeGameAmount = 100;
    public const ushort FakeGameRelationMultiplier = 4;
    public const ushort MinReleaseDate = 1970; // must at least be 1950 enforced by database
    public const ushort MaxReleaseDate = 2025;
    public const ushort MinRating = 1; // must at least be 1 enforced by database
    public const ushort MaxRating = 5; // must at max be 5 enforced by database
    public const ushort FakeRatingAmount = 1000;
}