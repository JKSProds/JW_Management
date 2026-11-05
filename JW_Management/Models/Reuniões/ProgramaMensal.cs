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
    public bool Faixa { get; set; }               // faixa de destaque (reunião de semana / de fim de semana)
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
    public string Congregacao { get; set; } = "";
    public string Mes { get; set; } = "";
    public List<ProgramaSecao> Secoes { get; set; } = new();
    public List<ProgramaSemana> Semanas { get; set; } = new();

    // Estrutura fixa do quadro; os rótulos dos indicadores e do sistema sonoro vêm da configuração do Hourglass.
    public static List<ProgramaSecao> CriarSecoes(Dictionary<string, string> r)
    {
        string R(string id, string padrao) => r.TryGetValue(id, out var v) && !string.IsNullOrWhiteSpace(v) ? v : padrao;
        ProgramaLinha L(string id, string rotulo) => new() { Id = id, Rotulo = rotulo };
        return new()
        {
            new() { Titulo = "REUNIÃO DE SEMANA", Faixa = true, Linhas = { L("pres", "Presidente"), L("orac", "Oração"), L("cmm1", "Cântico inicial"), L("cmm2", "Cântico do meio"), L("cmm3", "Cântico final") } },
            new() { Titulo = "TESOUROS DA PALAVRA DE DEUS", Linhas = { L("tes1", "Discurso"), L("tes2", "Em Busca de Pérolas Espirituais"), L("tes3", "Leitura da Bíblia") } },
            new() { Titulo = "EMPENHE-SE NO MINISTÉRIO", Linhas = { L("min1", "Parte 1"), L("min2", "Parte 2"), L("min3", "Parte 3"), L("min4", "Parte 4") } },
            new() { Titulo = "VIVER COMO CRISTÃOS", Linhas = { L("viv1", "Parte 1"), L("viv2", "Parte 2") } },
            new() { Titulo = "ESTUDO BÍBLICO", Linhas = { L("ebd", "Dirigente"), L("ebl", "Leitor"), L("ebo", "Oração final") } },
            new() { Titulo = "REUNIÃO DE FIM DE SEMANA", Faixa = true, Linhas = { L("wpr", "Presidente"), L("wor", "Orador"), L("wco", "Congregação"), L("wte", "Tema") } },
            new() { Titulo = "ESTUDO DE A SENTINELA", Linhas = { L("std", "Dirigente"), L("stl", "Leitor"), L("sto", "Oração final"), L("cwm1", "Cântico do meio"), L("cwm2", "Cântico final") } },
            new() { Titulo = "SERVIÇOS DA REUNIÃO", Faixa = true },
            new() { Titulo = "INDICADORES", Linhas = { L("ind1", R("ind1", "Receção")), L("ind2", R("ind2", "Auditório")), L("ind3", R("ind3", "Estacionamento")) } },
            new() { Titulo = "ÁUDIO / VÍDEO", Linhas = { L("som", R("som", "Som")), L("mic", "Micros"), L("tri", R("tri", "Tribuna")) } },
            new() { Titulo = "LIMPEZA", Linhas = { L("limp", R("limp", "Limpeza")) } }
        };
    }
}

// Serviço de campo: reuniões para o serviço (dia, hora, local, dirigente).
public class CampoEntrada
{
    public string Data { get; set; } = "";       // yyyy-MM-dd
    public string Hora { get; set; } = "";
    public string Local { get; set; } = "";
    public string Dirigente { get; set; } = "";
}

// Testemunho público: turnos por local (um ou dois publicadores por turno).
public class TestemunhoEntrada
{
    public string Data { get; set; } = "";       // yyyy-MM-dd (hora local)
    public string Inicio { get; set; } = "";     // HH:mm
    public string Fim { get; set; } = "";
    public string Local { get; set; } = "";
    public List<string> Pessoas { get; set; } = new();
}

public class ProgramaEstado
{
    public int Semanas { get; set; }
    public int SemanasEsperadas { get; set; }
    public bool Campo { get; set; }
    public bool Testemunho { get; set; }
    public bool Completo => Semanas >= SemanasEsperadas && SemanasEsperadas > 0 && Campo && Testemunho;
    public DateTime? Atualizado { get; set; }
}
