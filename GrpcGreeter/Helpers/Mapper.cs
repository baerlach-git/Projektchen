using Google.Protobuf.WellKnownTypes;
using GameServiceProtos;
using Shared.Models;


namespace GrpcGreeter.Helpers;


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
            Developer = gameDto.Developer,
            Platform = gameDto.Platform,
            Genre = gameDto.Genre,
            AverageRating = gameDto.AverageRating != null ? (float)gameDto.AverageRating.Value : null,
            CommentCount = (int)gameDto.CommentCount,
            UserRating =  gameDto.UserRating,
        };
    }

    public static GameRating MapToGameRating(this GameRatingDto ratingDto)
    {
        return new GameRating
        {
            GameId = ratingDto.GameId,
            Rating = ratingDto.Rating,
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

    public static Genre MapToGenre(this NameIdDto nameIdDto)
    {
        return new Genre
        {
            Id = nameIdDto.Id,
            Name = nameIdDto.Name
        };
    }
    
    public static Platform MapToPlatform(this NameIdDto nameIdDto)
    {
        return new Platform
        {
            Id = nameIdDto.Id,
            Name = nameIdDto.Name
        };
    }
    
    public static Developer MapToDeveloper(this NameIdDto nameIdDto)
    {
        return new Developer 
        {
            Id = nameIdDto.Id,
            Name = nameIdDto.Name
        };
    }
    
    public static Publisher MapToPublisher(this NameIdDto nameIdDto)
    {
        return new Publisher 
        {
            Id = nameIdDto.Id,
            Name = nameIdDto.Name
        };
    }
    
    
}

