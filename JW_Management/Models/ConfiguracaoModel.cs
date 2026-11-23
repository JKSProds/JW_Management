namespace JW_Management.Models;

public class ConfiguracaoModel
{
    public string? Authority { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; } // só preenchido para alterar; nunca é devolvido ao ecrã
    public bool ClientSecretDefinido { get; set; }
    public string? KnownProxy { get; set; }
    public bool SomenteOidc { get; set; }

    // Email da congregação ativa (colunas da congregação em sys_tenants)
    public string? EmailRemetente { get; set; }
    public string? EmailHost { get; set; }
    public string? EmailPorta { get; set; }
    public string? EmailPassword { get; set; } // só preenchido para alterar; nunca é devolvido ao ecrã
    public bool EmailPasswordDefinida { get; set; }

    // Email global (sys_config): nome do remetente e cópia de todos os emails
    public string? MailNome { get; set; }
    public string? MailBcc { get; set; }
}
