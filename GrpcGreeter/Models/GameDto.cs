namespace GrpcGreeter.Models;
public record GameDto(
    uint Id,
    string Name,
    ushort ReleaseDate,
    string Publisher,
    string Developer,
    string Platform,
    string Genre,
    float AverageRating
);