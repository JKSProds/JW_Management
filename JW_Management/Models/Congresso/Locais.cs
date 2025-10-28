namespace JW_Management.Models
{
    public class Locais
    {
        public string Morada { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public List<Congregacao> Congregacoes { get; set; }
        
        public int Recomendacoes => Congregacoes.Count(c => c.Recomendacao) == Congregacoes.Count ? 2 : (Congregacoes.Count(c => c.Recomendacao) > 0 ? 1 : 0);
    }
}