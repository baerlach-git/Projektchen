using Grpc.Core;
using GameServiceProtos;
using Google.Protobuf.WellKnownTypes;
using Grpcgreeter.Helpers;
using GrpcGreeter.Helpers;
using GrpcGreeter.Models;

namespace GrpcGreeter.Services;


public class GameService(GameRepository repo) : GameServiceProtos.GameService.GameServiceBase
{
  public override async Task<GameList> GetGames(EmptyMessage request, ServerCallContext context)
  {
    try
    {
      var gameDtos = await repo.GetGamesWithRatingsAsync();
      var gameList = gameDtos.Select(g => g.MapToGame());
      var response = new GameList { Games = { gameList } };
      return response;
    }
    
    catch(Exception ex)
    {
      Console.WriteLine("Exception in GetGamesAsync: " + ex.Message);
      throw new RpcException(new Status(StatusCode.Internal, "games could not be fetched"));
    }
  }

  public override async Task<Response> AddRating(GameRatingRequest request, ServerCallContext context)
  {
    try
    {
      await ExistanceChecker.CheckGameId(repo, request.GameId);

      var clientIp = GetClientIp(context);

      var rating = new GameRatingUpsertData
        {
          GameId = (int)request.GameId,
          Rating = (int)request.Rating,
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
    
    var clientIp = GetClientIp(context);

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

  public override async Task<Response> DeleteGameComment(DeleteGameCommentRequest request, ServerCallContext context)
  {
    await ExistanceChecker.CheckCommentId(repo,  request.CommentId, false);
    
    var clientIp = GetClientIp(context);
    var commentIp = await repo.GetGameCommentIpAsync(request.CommentId);

    if (clientIp != commentIp)
    {
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

  public override async Task<Response> UpdateGameComment(UpdateGameCommentRequest request, ServerCallContext context)
  {
    await ExistanceChecker.CheckCommentId(repo, request.CommentId, false);
    
    var clientIp = GetClientIp(context);
    var commentIp = await repo.GetGameCommentIpAsync(request.CommentId);

    if (clientIp != commentIp)
    {
      return new Response
      {
        Success = false,
        Message = "Cannot edit other users comments"
      };
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

  public override async Task<GameCommentList> GetAllCommentsForGame(GetAllCommentsForGameRequest request,
    ServerCallContext context)
  {
    await ExistanceChecker.CheckGameId(repo, request.GameId);
    
    var gameCommentDtos = await repo.GetGameCommentsForGameAsync(request.GameId);
    var gameCommentList = gameCommentDtos.Select(gc => gc.MapToGameComment());
    var response = new GameCommentList{GameComments = {gameCommentList}};
    return response;
  }


  private string GetClientIp(ServerCallContext context)
  {
    var httpContext = context.GetHttpContext();
    var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();

    if (clientIp == null)
    {
      //note sure which error code would be reasonable, RemoteIpAddress is only null for non TCP connections.
      //Is this case even possible considering I control the client/ in a GRPC Client in general?
      throw new RpcException(new Status(StatusCode.Aborted, "Only TCP connections are accepted"));
    }
    return clientIp;
  }
  
}
