namespace JW_Management.Models
{
    public class Rota
    {
        public string Id { get; set; }
        public Congresso Congresso { get; set; } = new Congresso();
        public TipoTransporte Tipo { get; set; }
        public string Nome { get; set; }
        public List<Viagem> Viagens { get; set; }
    }
}