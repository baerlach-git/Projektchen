using Google.Protobuf.WellKnownTypes;

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
            CommentCount = (int)gameDto.CommentCount,
        };
    }

    public static GameComment MapToGameComment(this GameCommentDto comment)
    {
        return new GameComment
        {
            Id = comment.Id,
            GameId = comment.GameId,
            ParentId = comment.ParentId,
            Content = comment.Content,
            Deleted = comment.Deleted,
            Edited = comment.Edited,
            CreatedAt = comment.CreatedAt.ToTimestamp(),
            UpdatedAt = comment.UpdatedAt.ToTimestamp(),
        };
    }
}

