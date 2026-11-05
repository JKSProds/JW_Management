namespace JW_Management.Models;

// Sincronização de publicadores com o Hourglass: pré-visualização e aplicação.
public class SyncCampo
{
    public string Campo { get; set; } = "";     // Email, Telemovel, Morada, Grupo
    public string Atual { get; set; } = "";
    public string Novo { get; set; } = "";
    public int GrupoId { get; set; }            // só para o campo Grupo
    public bool Aplicar { get; set; }
}

public class SyncLinha
{
    public int HgId { get; set; }
    public string HgNome { get; set; } = "";
    public string Cargo { get; set; } = "";      // nomeação / estado no Hourglass (só informativo)
    public int PubId { get; set; }               // 0 = ainda não existe na aplicação
    public string PubNome { get; set; } = "";
    public bool Novo => PubId == 0;
    public bool Aplicar { get; set; }
    public string Email { get; set; } = "";      // valores do Hourglass (usados ao criar)
    public string Telemovel { get; set; } = "";
    public string Morada { get; set; } = "";
    public int GrupoId { get; set; }
    public string GrupoNome { get; set; } = "";
    public List<SyncCampo> Campos { get; set; } = new();
}

public class SyncPessoa
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
}

public class SyncPreview
{
    public List<SyncLinha> Linhas { get; set; } = new();
    public List<SyncPessoa> SoLocal { get; set; } = new();
    public int SemAlteracoes { get; set; }
}
