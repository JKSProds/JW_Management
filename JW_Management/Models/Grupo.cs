namespace JW_Management.Models
{
    public class Grupo
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public Publicador? Responsavel { get; set; }
        public Publicador? Ajudante { get; set; }
    }
}
