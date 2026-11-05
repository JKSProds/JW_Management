namespace JW_Management.Models;

// Dados do ecrã «Programa»: o mês escolhido e o que já está guardado para ele.
public class ProgramaPagina
{
    public int Ano { get; set; }
    public int Mes { get; set; }
    public ProgramaEstado Estado { get; set; } = new();
    public ProgramaMensal? Programa { get; set; }
    public List<CampoEntrada>? Campo { get; set; }
    public List<TestemunhoEntrada>? Testemunho { get; set; }
}
