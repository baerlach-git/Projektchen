namespace GrpcGreeter.Models;
public class GameCommentUpsertData
{
    public int Id { get; set; }
    public int GameId { get; set; }
    public int? ParentId { get; set; }
    public string Ip {get; set;}
    public string Content { get; set;}
    public bool Deleted { get; set; }
}