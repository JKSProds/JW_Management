namespace JW_Management.Models;

// Dados do formulário de congregações (tenants). A ligação à BD nunca passa por aqui.
public class TenantForm
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public string NomeCongregacao { get; set; } = "";
    public string? Telefone { get; set; }
    public string? Morada { get; set; }
    public string? JWApiId { get; set; }
    public string? Email { get; set; }
    public string? EmailPassword { get; set; }   // vazio na edição = manter
    public string? EmailSMTPPort { get; set; }
    public string? EmailHost { get; set; }
    public string? AdminNome { get; set; }       // só na criação: primeiro administrador
    public string? AdminEmail { get; set; }
}
