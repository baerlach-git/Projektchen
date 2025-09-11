namespace GrpcGreeter.Models;
public record Game(
    uint Id,
    string Name,
    ushort ReleaseDate,
    string Publisher,
    string DevStudio,
    string Platform,
    string Genre,
    float AverageRating
);