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
        
        Task.Run(async () =>
        {
            bool success = await DeleteFilesOlder();
            if (success)
                Console.WriteLine("Apagados ficheiros com sucesso!");
            else
                Console.WriteLine("Erro ao apagar ficheiros.");
        });
    }

    public async Task<bool> DeleteFilesOlder()
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
                    Console.WriteLine($"Deleting file: {file}");
                    fileInfo.Delete(); // Delete the file
                }
            }

            Console.WriteLine("Cleanup completed.");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
            return false;
        }
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