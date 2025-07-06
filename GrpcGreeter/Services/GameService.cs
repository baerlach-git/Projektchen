using Grpc.Core;
using GrpcGameService;


public class GameService : GrpcGameService.GameService.GameServiceBase
{
  private readonly ExtendingBogus.GameRepository _repo;

  public GameService(ExtendingBogus.GameRepository repo)
  {
    _repo = repo;
  }

  public override async Task<GameList> GetGames(Empty request, ServerCallContext context)
  {
    var games = await _repo.GetAllAsync();

    var response = new GameList();
    response.Games.AddRange(games.Select(g => new GrpcGameService.Game
    {
      Id = g.Id,
      Name = g.Name,
      ReleaseDate = g.ReleaseDate.ToString(),
      Publicher = g.Publisher,
      DevStudio = g.DevStudio,
      Platform = g.Platform,
      Genre = g.Genre,
      CreatedAt = g.CreatedAt.ToString(),
      UpdatedAt = g.UpdatedAt.ToString(),
    }));

    return response;
  }
}
