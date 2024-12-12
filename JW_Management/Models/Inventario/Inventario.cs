namespace JW_Management.Models;

public class Inventario
{
    public string StampInventario { get; set; }
    public DateTime DataInventario { get; set; }
    public DateTime DataLimite { get; set; }
    public bool Atrasado => DateTime.Now > DataLimite;
    public List<Literatura> Literatura { get; set; }
}