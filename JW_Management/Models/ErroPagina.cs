namespace JW_Management.Models;

public class ErroPagina
{
    public string Pedido { get; set; } = "";
    public string? Caminho { get; set; }
    public string Resumo { get; set; } = "";
    public string? Detalhe { get; set; }
}
