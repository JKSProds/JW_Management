namespace JW_Management.Models
{
    public class Congregacao
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "NOT/DEFINED";
        public string Morada { get; set; }
        public string Telemovel { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public Congresso Congresso { get; set; }
        public List<Paragem> Paragens { get; set; }
        public List<Recomendacao> Recomendacoes { get; set; }
        public bool Recomendacao =>  Recomendacoes.Count > 0;
    }
}