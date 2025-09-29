namespace Shared.Models;

public class Pagination(ushort Limit, ushort Offset)
{
    public ushort Limit { get; set; }
    public ushort Offset { get; set; }
}