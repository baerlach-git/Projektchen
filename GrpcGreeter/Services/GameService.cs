using Google.Protobuf.Collections;

namespace GrpcGreeter.Services;

using Grpc.Core;
using GameServiceProtos;
using Models;
using Grpcgreeter.Helpers;


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

  public override async Task<GameRatingResponse> AddRating(GameRatingRequest request, ServerCallContext context)
  {
    try
    {
      var gameExists = await repo.GameExistsAsync(request.GameId);
      if (!gameExists)
      {
        throw new RpcException(new Status(StatusCode.NotFound, "The game cannot be found"));
      }

      var httpContext = context.GetHttpContext();
      var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();

      if (clientIp == null)
      {
        //note sure which error code would be reasonable, RemoteIpAddress is only null for non TCP connections.
        //Is this case even possible considering I control the client/ in a GRPC Client in general?
        throw new RpcException(new Status(StatusCode.Aborted, "Only TCP connections are accepted"));
      }

      var rating = new GameRatingUpsertData
      (
        request.GameId,
        clientIp,
        request.Rating
      );

      var ratingExists = await repo.RatingExistsAsync(request.GameId, clientIp);
      if (ratingExists)
      {
        var updateResponse = await repo.UpdateRatingAsync(rating);
        var updateGameRatingResponse = new GameRatingResponse
        {
          Success = updateResponse > 0,
          Message = updateResponse > 0 ? $"{updateResponse} game ratings updated" : "No game ratings not updated"
        };

        return updateGameRatingResponse;
      }
      
      var addRatingResponse = await repo.AddRatingAsync(rating);
      var addGameRatingResponse = new GameRatingResponse
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
}
