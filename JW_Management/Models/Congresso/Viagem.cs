namespace JW_Management.Models
{
    public class Viagem
    {
        public string Id { get; set; }
        public Congresso Congresso { get; set; } = new Congresso();
        public Rota Rota { get; set; }
        public string Destino { get; set; }
        public List<Viagem_Paragem> Paragens { get; set; }
    }
}