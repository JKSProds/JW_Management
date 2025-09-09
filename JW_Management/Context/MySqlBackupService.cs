namespace JW_Management.Models;
using MySql.Data.MySqlClient;


public class MySqlBackupService()
{
    private string _connectionString => _appContext._currentTenant.ConnectionString;
    private string _backupPath => "/app/img/backup/" + _appContext._currentTenant.Id + "/";
    public AppContext _appContext { get; set; }
    
    // Construtor com parâmetros
    public MySqlBackupService(AppContext appContext) : this()
    {
        _appContext = appContext;

        foreach (var t in _appContext._tenantContext._tenants)
        {
            _appContext._manualTenant = t.Id;
            
            if (BackupMySQL())
                Console.WriteLine($"[{t.NomeCongregacao}] Backup realizado com sucesso!");
            else
                Console.WriteLine($"[{t.NomeCongregacao}] Erro ao realizar backup.");
            
                if (DeleteFilesOlder())
                    Console.WriteLine($"[{t.NomeCongregacao}] Apagados ficheiros com sucesso!");
                else
                    Console.WriteLine($"[{t.NomeCongregacao}] Erro ao apagar ficheiros.");
        }

        _appContext._manualTenant = 0;
    }

    private bool DeleteFilesOlder()
    {
        try
        {
            // Get all files in the directory
            string[] files = Directory.GetFiles(_backupPath);

            // Iterate through the files
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);

                // Check if the file is older than 1 month
                if (fileInfo.LastWriteTime < DateTime.Now.AddMonths(-1))
                {
                    fileInfo.Delete(); // Delete the file
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
    
    private bool BackupMySQL()
    {
        try
        {
            string file = _backupPath + $"{DateTime.Now:ddMMyyyyHHmmss}_sys_jw.sql";
            using MySqlConnection conn = new MySqlConnection(_connectionString);
            using MySqlCommand cmd = new MySqlCommand();
            using MySqlBackup mb = new MySqlBackup(cmd);
            cmd.Connection = conn;
            conn.Open();
            mb.ExportToFile(file);
            conn.Close();

            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }
}