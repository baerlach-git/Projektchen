namespace GrpcGreeter.Models;

public interface IGameRelation
{
    public uint GameId { get; set; }
    public uint RelatedTableId { get; set; }
}