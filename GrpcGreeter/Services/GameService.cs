using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using GrpcGameService;


public class GameService : GrpcGameService.GameService.GameServiceBase
{
  private readonly ExtendingBogus.GameRepository _repo;

  public GameService(ExtendingBogus.GameRepository repo)
  {
    _repo = repo;
  }

  public override async Task<GameList> GetGames(GrpcGameService.Empty request, ServerCallContext context)
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
          CreatedAt = Timestamp.FromDateTime(g.CreatedAt.ToUniversalTime()),
          UpdatedAt = Timestamp.FromDateTime(g.UpdatedAt.ToUniversalTime())


        };

        grpcGame.Ratings.AddRange(g.Ratings.Select(r => new GrpcGameService.GameRating
        {
          Id = r.Id,
          Ip = r.Ip,
          Rating = r.Rating,
          GameId = r.GameId,
          CreatedAt = Timestamp.FromDateTime(r.CreatedAt.ToUniversalTime()),
          UpdatedAt = Timestamp.FromDateTime(r.UpdatedAt.ToUniversalTime())
        }));

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
        return new GameRatingResponse
        {
          Success = false,
          Message = "Game not found"
        };
      }

      var rating = new GameRating
      {
        GameId = request.GameId,
        Ip = request.Ip,
        Rating = request.Rating,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
      };

      var ratingExists = await _repo.RatingExistsAsync(request.GameId, request.Ip);

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
      throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
    }


  }
}
