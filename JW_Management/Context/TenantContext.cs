using System.Security.Claims;
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

                    using Database dbTenant = t.ConnectionString;

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
}