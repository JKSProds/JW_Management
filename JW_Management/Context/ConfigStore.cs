using Microsoft.AspNetCore.DataProtection;
using MySql.Data.MySqlClient;

namespace JW_Management.Models;

// Configuração global (BD master, tabela sys_config). O ClientSecret é guardado cifrado.
public class ConfigStore(string masterConnectionString, IDataProtectionProvider provider)
{
    public const string Authority = "Oidc:Authority";
    public const string ClientId = "Oidc:ClientId";
    public const string ClientSecret = "Oidc:ClientSecret";
    public const string KnownProxy = "ForwardedHeaders:KnownProxy";
    public const string SomenteOidc = "Auth:SomenteOidc";
    public const string MailNome = "Mail:Nome";           // nome do remetente (global)
    public const string MailBcc = "Mail:EmailBcc";        // cópia de todos os emails (global, opcional)

    private readonly IDataProtector _protector = provider.CreateProtector("JW_Management.Config.v1");

    // Palavras-passe guardadas fora de sys_config (ex.: email da congregação): cifradas com o prefixo «dp:».
    // Valores antigos, sem prefixo, são texto simples e continuam a funcionar. Se a chave de cifra não for a mesma, devolve vazio.
    public string Cifrar(string valor) => "dp:" + _protector.Protect(valor);
    public string Decifrar(string? valor)
    {
        if (string.IsNullOrEmpty(valor) || !valor.StartsWith("dp:")) return valor ?? "";
        try { return _protector.Unprotect(valor[3..]); } catch (Exception) { return ""; }
    }

    public void EnsureTable() =>
        Run("CREATE TABLE IF NOT EXISTS sys_config (Chave VARCHAR(100) NOT NULL PRIMARY KEY, Valor TEXT NOT NULL);");

    public Dictionary<string, string?> Load()
    {
        var values = new Dictionary<string, string?>();
        using var conn = new MySqlConnection(masterConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT Chave, Valor FROM sys_config", conn);
        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            var key = r.GetString(0);
            var value = r.GetString(1);
            try { values[key] = key == ClientSecret ? _protector.Unprotect(value) : value; }
            catch (Exception) { /* chave de cifra mudou: segredo ignorado, tem de ser reintroduzido */ }
        }
        return values;
    }

    // Valor vazio apaga a chave.
    public void Set(string key, string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Run("DELETE FROM sys_config WHERE Chave=@k", ("@k", key));
            return;
        }
        if (key == ClientSecret) value = _protector.Protect(value);
        Run("INSERT INTO sys_config (Chave, Valor) VALUES (@k, @v) ON DUPLICATE KEY UPDATE Valor=VALUES(Valor)",
            ("@k", key), ("@v", value));
    }

    // Lido a cada pedido (BD), para que "só OAuth" tenha efeito imediato; só vale se o OIDC estiver configurado.
    public bool SomenteOidcAtivo(IConfiguration config)
    {
#if DEBUG
        return false; // dev local: o segredo OIDC não desencripta com a chave local, por isso mantém-se o login com palavra-passe
#endif
        if (string.IsNullOrEmpty(config[ClientId])) return false;
        using var conn = new MySqlConnection(masterConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand("SELECT Valor FROM sys_config WHERE Chave=@k", conn);
        cmd.Parameters.AddWithValue("@k", SomenteOidc);
        return cmd.ExecuteScalar() as string == "true";
    }

    private void Run(string sql, params (string Name, string Value)[] parameters)
    {
        using var conn = new MySqlConnection(masterConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(sql, conn);
        foreach (var (name, value) in parameters) cmd.Parameters.AddWithValue(name, value);
        cmd.ExecuteNonQuery();
    }
}
