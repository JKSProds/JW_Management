using JW_Management.Models;
using MySql.Simple;
using AppContext = JW_Management.Models.AppContext;

namespace JW_Management.Component.DB;

using System.Globalization;

public class DbService
{
    private AppContext _appContext;
    private readonly string _migrationsPath = "Migrations";
    private double _currentVersion;

    public string _version => double.Parse(_currentVersion.ToString(CultureInfo.CurrentCulture)).ToString("0.00")
        .Replace(",", ".");


    public DbService(AppContext appContext)
    {
        _appContext = appContext;
        ApplyMigrations();
    }

    private void ApplyMigrations()
    {
        // Percorre todas as bases de dados para aplicar as migrações
        foreach (var t in _appContext._tenantContext._tenants)
        {
            Console.WriteLine($"[DB] A Verificar Migrações de DB para {t.Nome}");
            Database db = t.ConnectionString;

            // Obtém os scripts de migração em ordem crescente
            var scripts = Directory.GetFiles(Path.Combine(Directory.GetCurrentDirectory(), _migrationsPath), "*.sql")
                .OrderBy(f => f);

            // Aplica cada script pendente
            foreach (var scriptPath in scripts)
            {
                double scriptVersion = GetScriptVersion(scriptPath);
                if (scriptVersion > t.DbVersion)
                {
                    Console.WriteLine($"[DB] A Atualizar {t.Nome}, V.{t.DbVersion} -> {scriptVersion}");
                    string sql = File.ReadAllText(scriptPath);

                    db.Execute(sql);

                    // Atualiza a versão do banco após a execução do script
                    UpdateDatabaseVersion(_appContext._tenantContext._masterConnectionString, scriptVersion);
                    t.DbVersion = scriptVersion;
                }
            }

            _currentVersion = GetCurrentDatabaseVersion(t);
            Console.WriteLine($"[DB] Migrações de {t.Nome} executadas com sucesso. V.{_currentVersion}");

            db.Connection.Close();
        }
    }

    // Método para obter a versão atual do banco de dados
    private double GetCurrentDatabaseVersion(Tenant t)
    {
        string sqlQuery =
            $"SELECT DbVersion from sys_tenants WHERE Id={t.Id}";

        using Database db = _appContext._tenantContext._masterConnectionString;
        using var result = db.Query(sqlQuery);
        while (result.Read())
        {
            return double.Parse(result["DbVersion"], CultureInfo.InvariantCulture);
        }

        return 0;
    }

    // Método para atualizar a versão do banco de dados após uma migração
    private void UpdateDatabaseVersion(string connection, double version)
    {
        string sqlQuery =
            $"UPDATE sys_tenants set DbVersion={version.ToString().Replace(",", ".")}, DbUpdate='{DateTime.Now:yyyy-MM-dd HH:mm:ss}'";

        Database db = connection;

        db.Execute(sqlQuery);
        db.Connection.Close();
    }

    // Extrai a versão do nome do script, ex: "V_1.0_Inicio.sql" -> 1.0
    private double GetScriptVersion(string scriptPath)
    {
        var fileName = Path.GetFileName(scriptPath);

        // Exemplo: "V_1.1_AddicionadaNome.sql" -> "1.1"
        var versionPart = fileName.Split('_')[1];

        // Converte "1.1" para 1.1 como double
        return double.Parse(versionPart, CultureInfo.InvariantCulture);
    }
}