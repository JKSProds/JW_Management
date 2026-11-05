namespace JW_Management.Models;

public class ConfiguracaoModel
{
    public string? Authority { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; } // só preenchido para alterar; nunca é devolvido ao ecrã
    public bool ClientSecretDefinido { get; set; }
    public string? KnownProxy { get; set; }
    public bool SomenteOidc { get; set; }
}
