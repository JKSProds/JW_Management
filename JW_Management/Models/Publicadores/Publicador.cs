namespace JW_Management.Models
{
    public class Publicador
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Telemovel { get; set; }
        public Grupo? Grupo { get; set; }

        public List<Movimentos>? Movimentos { get; set; }
        public List<Literatura>? Pedidos { get; set; }
        public List<Territorio>? Territorios { get; set; }
    }
}
