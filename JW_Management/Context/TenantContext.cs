using System.Data.Common;
using System.Security.Claims;
using MySql.Data.MySqlClient;
using MySql.Simple;

namespace JW_Management.Models;

public class TenantContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public List<Tenant> _tenants { get; set; }

    public Tenant _currentTenant => _tenants.Where(t => t.Id == GetUserTenant())
        .DefaultIfEmpty(new Tenant() { ConnectionString = _masterConnectionString }).First();

    public string _masterConnectionString { get; set; }

    public TenantContext(IHttpContextAccessor httpContextAccessor, string masterConnectionString)
    {
        _httpContextAccessor = httpContextAccessor;
        _masterConnectionString = masterConnectionString;

        _tenants = ObterListaTenants();
    }

    public int GetUserTenant()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity is { IsAuthenticated: true })
        {
            var value = user.FindFirst(ClaimTypes.Sid)?.Value;
            if (value != null)
            {
                int t = int.Parse(value);
                return t;
            }
        }

        return 0;
    }

    public List<Tenant> ObterListaTenants()
    {
        List<Tenant> LstTenants = new List<Tenant>();

        string sqlQuery = "SELECT * from sys_tenants where Enable=1;";

        try
        {
            using Database db = _masterConnectionString;
            using var result = db.Query(sqlQuery);
            while (result.Read())
            {
                Tenant t = new Tenant();
                try
                {
                    t = new Tenant()
                    {
                        Id = result["Id"],
                        Nome = result["Nome"],
                        ConnectionString = result["ConnectionString"],
                        DbVersion = double.Parse(result["DbVersion"], System.Globalization.CultureInfo.InvariantCulture),
                        NomeCongregacao = result["NomeCongregacao"],
                        TelefoneCongregacao = result["TelefoneCongregacao"],
                        MoradaCongregacao = result["MoradaCongregacao"],
                        JWApiIdCongregacao = result["JWApiCongregacao"],
                        Email = result["MailSender"],
                        EmailPassword = result["MailSenderPassword"],
                        EmailSMTPPort = result["MailSenderPort"],
                        EmailHost = result["MailSenderHost"]
                    };
                    
                    var dbTenantName = $"sys_jw_{t.Id}";
                    using Database dbTenant = _masterConnectionString;
 
                    dbTenant.Execute($"CREATE DATABASE IF NOT EXISTS {dbTenantName};");

                    t.ConnectionString += "database=" + dbTenantName;
                    LstTenants.Add(t);
                }
                catch
                {
                    Console.WriteLine($"Não foi possivel adicionar o tenant: {t.Nome}");
                }
            }
        }
        catch
        {
            Console.WriteLine($"Não foi possivel obter os tenants! ConnectionString: {_masterConnectionString}");
        }

        return LstTenants;
    }

    // Tenant principal: o único cujos Admins gerem congregações. Chave Tenants:Principal em sys_config; sem ela, o de menor Id.
    public int IdPrincipal(IConfiguration c) => int.TryParse(c["Tenants:Principal"], out var p) ? p : _tenants.Select(t => t.Id).DefaultIfEmpty(0).Min();

    public void Recarregar() => _tenants = ObterListaTenants();

    private void Executar(string sql, params (string, object?)[] ps)
    {
        using var conn = new MySqlConnection(_masterConnectionString);
        conn.Open();
        using var cmd = new MySqlCommand(sql, conn);
        foreach (var (k, v) in ps) cmd.Parameters.AddWithValue(k, v ?? "");
        cmd.ExecuteNonQuery();
    }

    private (string, object?)[] Campos(TenantForm f) => new (string, object?)[]
    {
        ("@nome", f.Nome.Trim()), ("@cong", f.NomeCongregacao.Trim()), ("@tel", f.Telefone?.Trim()), ("@mor", f.Morada?.Trim()), ("@api", f.JWApiId?.Trim()),
        ("@mail", f.Email?.Trim()), ("@mport", f.EmailSMTPPort?.Trim()), ("@mhost", f.EmailHost?.Trim()), ("@mpass", f.EmailPassword)
    };

    // A ligação à BD do novo tenant copia a do tenant principal (mesmo servidor); nunca é pedida nem mostrada no formulário.
    public int Criar(TenantForm f, int idPrincipal)
    {
        string? baseConn;
        using (var conn = new MySqlConnection(_masterConnectionString))
        {
            conn.Open();
            using var c = new MySqlCommand("SELECT ConnectionString FROM sys_tenants WHERE Id=@i", conn);
            c.Parameters.AddWithValue("@i", idPrincipal);
            baseConn = c.ExecuteScalar() as string;
        }
        if (string.IsNullOrWhiteSpace(baseConn)) throw new InvalidOperationException("Tenant principal sem ligação à base de dados.");
        var ps = Campos(f).Append(("@conn", baseConn)).ToArray();
        Executar("INSERT INTO sys_tenants (Id, Nome, ConnectionString, DbVersion, NomeCongregacao, TelefoneCongregacao, MoradaCongregacao, JWApiCongregacao, MailSender, MailSenderPassword, MailSenderPort, MailSenderHost, Enable) " +
                 "SELECT IFNULL(MAX(Id), 0) + 1, @nome, @conn, 0, @cong, @tel, @mor, @api, @mail, @mpass, @mport, @mhost, 1 FROM sys_tenants", ps);
        using var conn2 = new MySqlConnection(_masterConnectionString);
        conn2.Open();
        using var c2 = new MySqlCommand("SELECT MAX(Id) FROM sys_tenants", conn2);
        return Convert.ToInt32(c2.ExecuteScalar());
    }

    // Palavra-passe do email vazia mantém a atual.
    public void Atualizar(TenantForm f)
    {
        var ps = Campos(f).Append(("@id", (object?)f.Id)).ToArray();
        Executar("UPDATE sys_tenants SET Nome=@nome, NomeCongregacao=@cong, TelefoneCongregacao=@tel, MoradaCongregacao=@mor, JWApiCongregacao=@api, MailSender=@mail, MailSenderPort=@mport, MailSenderHost=@mhost" +
                 (string.IsNullOrEmpty(f.EmailPassword) ? "" : ", MailSenderPassword=@mpass") + " WHERE Id=@id", ps);
    }
}
