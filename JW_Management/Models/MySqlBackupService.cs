namespace JW_Management.Models;
using MySql.Data.MySqlClient;


public class MySqlBackupService()
{
    private readonly string _connectionString;
    private readonly string _backupPath;
    
    // Construtor com parâmetros
    public MySqlBackupService(string connectionString, string backupPath) : this()
    {
        _connectionString = connectionString;
        _backupPath = backupPath;

        // Executar o backup automaticamente ao instanciar a classe
        Task.Run(async () =>
        {
            bool success = await BackupMySQL();
            if (success)
                Console.WriteLine("Backup realizado com sucesso!");
            else
                Console.WriteLine("Erro ao realizar backup.");
        });
    }
    
    public async Task<bool> BackupMySQL()
    {
        try
        {
            string file = _backupPath + $"{DateTime.Now:ddMMyyyyHHmmss}_sys_jw.sql";
            await using MySqlConnection conn = new MySqlConnection(_connectionString);
            await using MySqlCommand cmd = new MySqlCommand();
            using MySqlBackup mb = new MySqlBackup(cmd);
            cmd.Connection = conn;
            conn.Open();
            mb.ExportToFile(file);
            await conn.CloseAsync();

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro durante o backup: {ex.Message}");
            return false;
        }
    }
}