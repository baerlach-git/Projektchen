using Bogus.DataSets;
using Google.Protobuf.WellKnownTypes;

public class GameRating
{
  public int Id { get; set; }

  public int GameId { get; set; } // Foreign key

  public string Ip { get; set; }
  public int Rating { get; set; }  // is it possible to constrain values here? e.g. from 1 to 5?

  public string Comment { get; set; }

  public DateTime CreatedAt { get; set; }
  public DateTime UpdatedAt { get; set; }

}