namespace GrpcGreeter.Models;

public class GameRatingUpsertData
{
    public int GameId  { get; set; }
    public string Ip { get; set; }
    public int Rating  { get; set; }
}