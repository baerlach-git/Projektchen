using Bogus.DataSets;
using Google.Protobuf.WellKnownTypes;

public class Game
{
  public int Id { get; set; }
  public string Name { get; set; }
  public int ReleaseDate { get; set; }

  public string Publisher { get; set; }
  public string DevStudio { get; set; }

  public string Platform { get; set; }

  public string Genre { get; set; }
  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

  public List<GameRating> Ratings { get; set; } = new();

}