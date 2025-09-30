namespace Shared.Models;

public class GameWithRating
{
   public int Id  { get; set; }
   public string Name   { get; set; }
   public int ReleaseDate  { get; set; }
   public string Publisher   { get; set; }
   public string Developer   { get; set; }
   public string Platform    { get; set; }
   public string Genre     { get; set; }
   public int? Rating  { get; set; }
   public float? AverageRating  { get; set; }
   public long CommentCount   { get; set; }
}


