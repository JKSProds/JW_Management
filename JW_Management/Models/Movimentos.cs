namespace JW_Management.Models
{
    public class Movimentos
    {
        public string? Stamp { get; set; }
        public Literatura? Literatura { get; set; }
        public int Quantidade { get; set; }
        public DateTime DataMovimento { get; set; }
        public Publicador? Publicador { get; set; }
    }
}
