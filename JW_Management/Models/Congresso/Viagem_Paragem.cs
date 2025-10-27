namespace JW_Management.Models
{
    public class Viagem_Paragem
    {
        public Viagem Viagem { get; set; }
        public Congresso Congresso { get; set; } = new Congresso();
        public Paragem Paragem { get; set; }
        public DateTime DataPartida { get; set; }
        public int Sequencia { get; set; }
    }
}