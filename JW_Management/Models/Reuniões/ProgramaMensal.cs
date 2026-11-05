namespace JW_Management.Models;

// Programa mensal de atividades: uma coluna por semana, linhas agrupadas por secção.
public class ProgramaLinha
{
    public string Id { get; set; } = "";
    public string Rotulo { get; set; } = "";
}

public class ProgramaSecao
{
    public string Titulo { get; set; } = "";
    public bool Faixa { get; set; }               // faixa azul (reunião de semana / de fim de semana)
    public List<ProgramaLinha> Linhas { get; set; } = new();
}

public class ProgramaSemana
{
    public DateTime Inicio { get; set; }
    public string Titulo { get; set; } = "";       // «Semana de 2 a 8 de Novembro»
    public List<string> Evento { get; set; } = new(); // palavras do evento (assembleia…), vazio = sem evento
    public Dictionary<string, string> Valores { get; set; } = new();
}

public class ProgramaMensal
{
    public string Titulo { get; set; } = "";
    public List<ProgramaSecao> Secoes { get; set; } = new();
    public List<ProgramaSemana> Semanas { get; set; } = new();
}
