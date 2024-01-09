namespace JW_Management.Models
{
    public class Cantico
    {
        public string Stamp { get; set; }
        public int? Id { get; set; }
        public string? Nome { get; set; }
        public string? TextoBiblico { get; set; }

        public Cantico() {
            this.Stamp = DateTime.Now.Ticks.ToString();
            this.Id = 0;
            this.Nome = "N/D";
            this.TextoBiblico = "N/D";
        }
    }
}
