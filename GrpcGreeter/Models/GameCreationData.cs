namespace GrpcGreeter.Models;

public class GameCreationData
{
    public required string Name { get; set; }
    public required int ReleaseDate { get; set; }
    public required int PublisherId { get; set; }
    public required int DeveloperId { get; set; }
    public required int[] GenreIds { get; set; }
    public required int[] PlatformIds { get; set; }
}