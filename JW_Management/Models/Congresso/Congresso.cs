namespace JW_Management.Models
{
    public class Congresso
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "NOT/DEFINED";
        public string Local { get; set; }
        public DateTime DataInicio { get; set; }
        public DateTime DataFim { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }

        public List<Congregacao> Congregacoes { get; set; } = new List<Congregacao>();
    }
}