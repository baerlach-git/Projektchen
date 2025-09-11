namespace GrpcGreeter.Services;

using Grpc.Core;
using GrpcGameService;


public class GameService : GrpcGameService.GameService.GameServiceBase
{
  private readonly GameRepository _repo;

  public GameService(GameRepository repo)
  {
    _repo = repo;
  }

  public override async Task<GameList> GetGames(Empty request, ServerCallContext context)
  {
    try
    {
      var games = await _repo.GetGamesWithRatingsAsync();

      var response = new GameList();

      foreach (var g in games)
      {
        var grpcGame = new GrpcGameService.Game
        {
          Id = g.Id,
          Name = g.Name,
          ReleaseDate = g.ReleaseDate,
          Publisher = g.Publisher,
          DevStudio = g.DevStudio,
          Platform = g.Platform,
          Genre = g.Genre,
          AverageRating = g.AverageRating,
        };


        response.Games.Add(grpcGame);
      }

      return response;
    }
    catch (Exception ex)
    {
      Console.WriteLine("Exception in GetGamesAsync: " + ex.Message);
      throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));

    }
  }

  public override async Task<GameRatingResponse> AddRating(GameRatingRequest request, ServerCallContext context)
  {
    try
    {
      var gameExists = await _repo.GameExistsAsync(request.GameId);
      if (!gameExists)
      {
        throw new RpcException(new Status(StatusCode.NotFound, "The game does not exist"));
      }

      var httpContext = context.GetHttpContext();
      var clientIp = httpContext.Connection.RemoteIpAddress?.ToString();
      Console.WriteLine(clientIp);

      if (clientIp == null)
      {
        //note sure which error code would be reasonable, RemoteIpAddress is only null for non TCP connections. Is this case even possible considering I control the client/ in a GRPC Client in general?
        throw new RpcException(new Status(StatusCode.Aborted, "Only TCP connections are accepted"));
      }

      var rating = new GameRating
      {
        GameId = request.GameId,
        Ip = clientIp,
        Rating = request.Rating,
      };

      var ratingExists = await _repo.RatingExistsAsync(request.GameId, clientIp);

      if (ratingExists)
      {
        await _repo.UpdateRatingAsync(rating);

        return new GameRatingResponse
        {
          Success = true,
          Message = "Rating updated succesfully"
        };
      }

      else
      {
        await _repo.AddRatingAsync(rating);

        return new GameRatingResponse
        {
          Success = true,
          Message = "Rating added successfully"
        };

      }

    }
    catch (Exception ex)
    {
      Console.WriteLine("Error in AddRating: " + ex.Message);
      throw new RpcException(new Status(StatusCode.Internal, "Rating insert/update failed"));
    }


  }
}
