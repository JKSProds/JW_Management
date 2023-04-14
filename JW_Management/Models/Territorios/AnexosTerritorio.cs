namespace JW_Management.Models
{
    public class AnexosTerritorio
    {
        public string? Stamp { get; set; }
        public string? NomeFicheiro { get; set; }
        public string? CaminhoFicheiro { get; set; }
        public string? Extensao { get; set; }
        public string? Descricao { get; set; }
        public Territorio? Territorio { get; set; }
    }
}
