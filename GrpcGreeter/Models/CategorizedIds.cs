namespace GrpcGreeter.Models;

public class CategorizedIds
{
    public List<int> added { get; set; }
    public List<int> deleted { get; set; }
    public List<int> unchanged { get; set; }
};