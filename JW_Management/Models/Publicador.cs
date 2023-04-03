namespace JW_Management.Models
{
    public class Publicador
    {
        public int Id { get; set; }
        public string? Nome { get; set; }

        public List<Movimentos>? Movimentos { get; set; }
        public List<Literatura>? Pedidos { get; set; }
    }
}
