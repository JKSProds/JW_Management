namespace JW_Management.Models;

// Modelos da importação do Hourglass. Servem de resposta da pré-visualização e de corpo da importação.
public class HgPessoa
{
    public string Nome { get; set; } = "";
    public int PubId { get; set; }          // 0 = sem correspondência nos publicadores locais
    public string PubNome { get; set; } = "";
}

public class HgItem
{
    public string Chave { get; set; } = "";     // ex.: mm:chairman, mm:fm:initcall, wm:reader
    public string Grupo { get; set; } = "mm";   // mm = meio de semana, wm = fim de semana
    public string Titulo { get; set; } = "";
    public string Rotulo { get; set; } = "";    // nome genérico do tipo (não muda de semana para semana)
    public int Base { get; set; }               // 0, 100, 200, 300, 400: separador da reunião onde o tipo fica
    public bool Salas { get; set; }             // o tipo tem sala adicional
    public string Tema { get; set; } = "";      // texto a gravar em «Designação»; vazio mantém o da reunião-base
    public int Minutos { get; set; }
    public string Sala { get; set; } = "Auditório";
    public List<HgPessoa> Pessoas { get; set; } = new();
    public int TipoId { get; set; }             // tipo local (r_tipos): >0 existente, -1 = criar a partir do Hourglass, 0 = não importar
}

public class HgSemana
{
    public string Stamp { get; set; } = "";
    public string Semana { get; set; } = "";
    public string Data { get; set; } = "";
    public bool Existe { get; set; }
    public bool Importar { get; set; } = true;
    public List<int> Canticos { get; set; } = new();
    public List<string> CanticoTitulos { get; set; } = new();   // títulos, na mesma ordem dos números
    public List<HgItem> Itens { get; set; } = new();
}

public class HgTipo
{
    public int Id { get; set; }
    public string Descricao { get; set; } = "";
    public string Adicional { get; set; } = "";
}

public class HgPreview
{
    public List<HgSemana> Semanas { get; set; } = new();
    public List<HgTipo> Tipos { get; set; } = new();
}
