namespace JW_Management.Models
{
    public class Paragem
    {
        public string Id { get; set; }
        public Congresso Congresso { get; set; } = new Congresso();
        public TipoTransporte Tipo { get; set; }
        public string CodParagem { get; set; }
        public string NomeParagem { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string IdZona { get; set; }
        public string Url { get; set; }
        public int Distancia { get; set; }
        public List<Viagem_Paragem> Viagens { get; set; }
    }
}