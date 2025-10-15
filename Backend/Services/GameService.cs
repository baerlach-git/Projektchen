using Grpc.Core;
using GameServiceProtos;
using GrpcGreeter.Helpers;
using Shared.Models;

namespace GrpcGreeter.Services;


public class GameService(GameRepository repo) : GameServiceProtos.GameService.GameServiceBase
{
  public override async Task<Games> GetGames(EmptyMessage request, ServerCallContext context)
  {
    try
    {
      var clientIp = GameServiceHelpers.GetClientIp(context);
      var gameDtos = await repo.GetGamesAndRatingsAsync(clientIp);
      var games = gameDtos.Select(g => g.MapToGame());
      var response = new Games { Games_ = { games } };
      return response;
    }
    
    catch(Exception ex)
    {
      Console.WriteLine("Exception in GetGamesAsync: " + ex.Message);
      throw;
    }
  }
  public override async Task<GameWithCommentsAndRating?> GetGameWithCommentsAndRating(GameWithCommentsAndRatingRequest request, ServerCallContext context)
  {
    try
    {
      var clientIp = GameServiceHelpers.GetClientIp(context);
      
      var gameExists = await repo.IdExistsInTableAsync(request.GameId, TableNames.Game);
      if (!gameExists)
      {
        return null;
      }

      var game = await repo.GetGameAsync(request.GameId, clientIp); //contains commentCount which isn't used
      var commentDtos = await repo.GetGameCommentsForGameAsync(request.GameId);
      
      var response = new GameWithCommentsAndRating
      {
        Id = game.Id,
        Name = game.Name,
        ReleaseDate = game.ReleaseDate,
        Publisher = game.Publisher,
        DevStudio = game.Developer,
        Platform = game.Platform,
        Genre = game.Genre,
        AverageRating = game.AverageRating != null ? (float?)game.AverageRating.Value : null,
        UserRating = game.UserRating,
        Comments = { commentDtos.Select(c => c.MapToGameComment()) },
      };
      return response;
    }
    catch (Exception ex)
    {
      Console.WriteLine("Exception in GetGameWithCommentsAndRating: " + ex.Message);
      throw new RpcException(new Status(StatusCode.Internal, "games could not be fetched"));
    }
    
  }
  
  public override async Task<Response> AddGame(AddGameRequest request, ServerCallContext context)
  {
    var gameCreationData = new GameCreationData
    {
      Name = request.Name,
      ReleaseDate = request.ReleaseDate,
      PublisherId = request.PublisherId,
      DeveloperId = request.DeveloperId,
      GenreIds = request.GenreIds.ToList(),
      PlatformIds = request.PlatformIds.ToList(),
    };

    var gameAlreadyExists = await repo.GameToInsertExistsAsync(gameCreationData);

    if (gameAlreadyExists)
    {
      throw new RpcException(new Status(StatusCode.AlreadyExists, "game already exists"));
    }
    try
    {
      var insertedRows = await repo.AddGame(gameCreationData);
      return new Response
      {
        Success = insertedRows.insertedGamesCount > 0,
        Message =
          $"Added {insertedRows.insertedGamesCount} games, {insertedRows.insertedGenreRelationsCount} game genre relations and {insertedRows.insertedPlatformRelationsCount} game platform relations"
      };
    }
    catch(Exception ex)
    {
      Console.WriteLine("Exception in AddGame: " + ex.Message);
      throw new RpcException(new Status(StatusCode.Internal, "games could not be added"));
    }
    
  }

  public override async Task<Response> UpdateGame(UpdateGameRequest request, ServerCallContext context)
  {
    var gameCreationData = new GameCreationData()
    {
      Name = request.Name,
      ReleaseDate = request.ReleaseDate,
      PublisherId = request.PublisherId,
      DeveloperId = request.DeveloperId,
      GenreIds = request.GenreIds.ToList(),
      PlatformIds = request.PlatformIds.ToList(),
    };
    
    var gameAlreadyExists = await repo.GameToInsertExistsAsync(gameCreationData);

    if (gameAlreadyExists)
    {
      throw new RpcException(new Status(StatusCode.AlreadyExists,
        "There already exists another game with the updated data"));
    }

    try
    {
      var changedRows = await repo.UpdateGame(request.GameId, gameCreationData);

      return new Response
      {
        Success = changedRows.updatedGamesCount > 0,
        Message =
          $@"Updated {changedRows.updatedGamesCount} games, deleted {changedRows.Item2.deletedPlatformRelationsCount} platform relations,
        added {changedRows.Item2.addedPlatformRelationsCount} platform relations, left {changedRows.Item2.unchangedPlatformRelationsCount} platform relations unchanged, 
        deleted {changedRows.Item3.deletedGenreRelationsCount} genre relations, added {changedRows.Item3.addedGenreRelationsCount} genre relations,
        left {changedRows.Item3.unchangedGenreRelationsCount} genre relations unchanged,",
      };
    }
    catch (Exception ex)
    {
      Console.WriteLine("Exception in UpdateGame: " + ex.Message);
      throw new RpcException(new Status(StatusCode.Internal, "games could not be updated"));
    }
    
  }




  public override async Task<Response> DeleteGame(DeleteGameRequest request, ServerCallContext context)
  {
    var gameExists = await repo.IdExistsInTableAsync(request.GameId, TableNames.Game);
    if (!gameExists)
    {
      //maybe use an exception anyway? The DeleteGame method would be called via a Button in the frontend on a game page
      //or in a game list for a specific game, so the game should definitely exist. If it cannot be found, something must have went wrong
      
      //throw new RpcException(new Status(StatusCode.NotFound, "The game cannot be found"));
      return new Response()
      {
        Success = false,
        Message = $"Game with id {request.GameId} not found"
      };
    }

    try
    {


      var deleteGameResponse = await repo.DeleteGameAsync(request.GameId);

      return new Response
      {
        Success = deleteGameResponse.deletedGamesCount > 0,
        Message =
          $@"Deleted {deleteGameResponse.deletedGamesCount} games, 
        {deleteGameResponse.deletedGameGenreRelationsCount} game genre relations, 
        {deleteGameResponse.deletedGamePlatformRelationsCount} game platform relations,
        {deleteGameResponse.deletedCommentsCount} comments and
        {deleteGameResponse.deletedGameRatingsCount} ratings."
      };
    }
    catch (Exception ex)
    {
      Console.WriteLine("Exception in DeleteGame: " + ex.Message);
      throw new RpcException(new Status(StatusCode.Internal, "games could not be deleted"));
    }
  }


  public override async Task<Response> AddRating(GameRatingRequest request, ServerCallContext context)
  {
    await ExistanceChecker.CheckGameId(repo, request.GameId);

    try
    {

      var clientIp = GameServiceHelpers.GetClientIp(context);

      var rating = new GameRatingUpsertData
        {
          GameId = request.GameId,
          Rating = request.Rating,
          Ip = clientIp,
        };

      var ratingExists = await repo.RatingExistsAsync(request.GameId, clientIp);
      if (ratingExists)
      {
        var updateResponse = await repo.UpdateRatingAsync(rating);
        var updateGameRatingResponse = new Response
        {
          Success = updateResponse > 0,
          Message = updateResponse > 0 ? $"{updateResponse} game ratings updated" : "No game ratings not updated"
        };

        return updateGameRatingResponse;
      }
      
      var addRatingResponse = await repo.AddRatingAsync(rating);
      var addGameRatingResponse = new Response
      {
        Success = addRatingResponse > 0,
        Message = addRatingResponse > 0 ? $"{addRatingResponse} game ratings were added" : "No game ratings were added"
      };
      return addGameRatingResponse;
    }
    
    catch (Exception ex)
    {
      Console.WriteLine("Error in AddRating: " + ex.Message);
      throw new RpcException(new Status(StatusCode.Internal, "Rating insert/update failed"));
    }
  }

  public override async Task<Response> AddGameComment(AddGameCommentRequest request, ServerCallContext context)
  {
    await ExistanceChecker.CheckGameId(repo, request.GameId);

    if (request.ParentId != null)
    {
      await ExistanceChecker.CheckCommentId(repo, request.ParentId, true);
    }
    
    try
    {
      
      var clientIp = GameServiceHelpers.GetClientIp(context);

      var comment = new GameCommentUpsertData
      {
        GameId = request.GameId,
        ParentId = request.ParentId,
        Ip = clientIp,
        Content = request.Content,
      };

      var addCommentResponse = await repo.AddGameCommentAsync(comment);
      return new Response
      {
        Success = addCommentResponse > 0,
        Message = addCommentResponse > 0 ? "Comment added" : "No comment added"
      };
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error in AddGameComment: " + ex.Message);
      throw new RpcException(new Status(StatusCode.Internal, "Add game comment failed"));
    }
    
  }

  public override async Task<Response> DeleteGameComment(DeleteGameCommentRequest request, ServerCallContext context)
  {
    await ExistanceChecker.CheckCommentId(repo,  request.CommentId, false);

    try
    {
      var clientIp = GameServiceHelpers.GetClientIp(context);
      var commentIp = await repo.GetGameCommentIpAsync(request.CommentId);

      if (clientIp != commentIp)
      {//exception
        return new Response
        {
          Success = false,
          Message = "Cannot delete other users comments"
        };
      }

      if (request.ParentId != null)
      {
        var parentResponse = await repo.SoftDeleteGameCommentAsync(request.CommentId);
        return new Response
        {
          Success = parentResponse > 0,
          Message = parentResponse > 0 ? "Comment deleted" : "No comment deleted"
        };
      }

      var response = await repo.HardDeleteGameCommentAsync(request.CommentId);
      return new Response
      {
        Success = response > 0,
        Message = response > 0 ? "Comment deleted" : "No comment deleted"
      };
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error in DeleteGameComment: " + ex.Message);
      throw new RpcException(new Status(StatusCode.Internal, "Delete game comment failed"));
    }
  }

  public override async Task<Response> UpdateGameComment(UpdateGameCommentRequest request, ServerCallContext context)
  {
    await ExistanceChecker.CheckCommentId(repo, request.CommentId, false);

    try
    {
      var clientIp = GameServiceHelpers.GetClientIp(context);
      var commentIp = await repo.GetGameCommentIpAsync(request.CommentId);

      if (clientIp != commentIp)
      {
        throw new RpcException(new Status(StatusCode.PermissionDenied, "caller Ip does not match commenter Ip"));
      }

      var comment = new GameCommentUpsertData
      {
        Id = request.CommentId,
        Content = request.Content,
      };

      var response = await repo.UpdateGameCommentAsync(comment);

      return new Response
      {
        Success = response > 0,
        Message = response > 0 ? "Comment updated" : "No comment updated"
      };
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error in UpdateGameComment: " + ex.Message);
      throw new RpcException(new Status(StatusCode.Internal, "Update game comment failed"));
    }
    
  }

  public override async Task<GameCommentList> GetAllCommentsForGame(GetAllCommentsForGameRequest request,
    ServerCallContext context)
  {
    await ExistanceChecker.CheckGameId(repo, request.GameId);

    try
    {
      var gameCommentDtos = await repo.GetGameCommentsForGameAsync(request.GameId);
      var gameCommentList = gameCommentDtos.Select(gc => gc.MapToGameComment());
      var response = new GameCommentList { GameComments = { gameCommentList } };
      return response;
    }
    catch (Exception ex)
    {
      Console.WriteLine("Error in GetAllCommentsForGame: " + ex.Message);
      throw new RpcException(new Status(StatusCode.Internal, "GetAll comments failed"));
    }
  }

  public override async Task<GameCreationPresets> GetGameCreationPresets(EmptyMessage request, ServerCallContext context)
  {
    try
    {
      var developerDtos = await repo.GetAllDevelopersAsync();
      var publisherDtos = await repo.GetAllPublishersAsync();
      var genreDtos =  await repo.GetAllGenresAsync();
      var platformDtos = await repo.GetAllPlatformsAsync();
    
      var developers = developerDtos.Select(d => d.MapToDeveloper());
      var publishers = publisherDtos.Select(d => d.MapToPublisher());
      var genres =  genreDtos.Select(d => d.MapToGenre());
      var platforms = platformDtos.Select(d => d.MapToPlatform());

      var response = new GameCreationPresets
      {
        Developers = { developers },
        Publishers = { publishers },
        Genres = { genres },
        Platforms = { platforms }
      };
    
      return response;
    }
    catch(Exception ex)
    {
      Console.WriteLine("Error in GetGameCreationPresets: " + ex.Message);
      throw new RpcException(new Status(StatusCode.Internal, "GetGameCreationPresets failed"));
    }
  }
  
}
