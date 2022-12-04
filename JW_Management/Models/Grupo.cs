namespace JW_Management.Models
{
    public class Grupo
    {
        public int Id { get; set; }
        public string? Nome { get; set; }
        public string? Responsavel { get; set; }
        public string? Ajudante { get; set; }

        public Grupo() {
            Nome = "ND";
            Responsavel = "ND";
            Ajudante = "ND";
        }
    }
}
