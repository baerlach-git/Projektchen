namespace Shared.Models;
public record GameDto(
    int Id,
    string Name,
    int ReleaseDate,
    string Publisher,
    string Developer,
    string Platform,
    string Genre,
    decimal AverageRating,
    long CommentCount
);