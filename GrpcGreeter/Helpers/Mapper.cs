namespace Grpcgreeter.Helpers;

using GameServiceProtos;
using GrpcGreeter.Models;

public static class Mapper
{
    public static Game MapToGame(this GameDto gameDto)
    {
        return new Game
        {
            Id = gameDto.Id,
            Name = gameDto.Name,
            ReleaseDate = gameDto.ReleaseDate,
            Publisher = gameDto.Publisher,
            DevStudio = gameDto.Developer,
            Platform = gameDto.Platform,
            Genre = gameDto.Genre,
            AverageRating = (float)gameDto.AverageRating,
        };
    }
}

