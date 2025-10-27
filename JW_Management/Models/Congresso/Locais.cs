namespace JW_Management.Models
{
    public class Locais
    {
        public string Morada { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public List<Congregacao> Congregacoes { get; set; }
    }
}