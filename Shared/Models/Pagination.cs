namespace Shared.Models;

public class Pagination(ushort Limit, ushort Offset)
{
    public required ushort Limit { get; set; }
    public required ushort Offset { get; set; }
}