namespace JW_Management.Models
{
    public class TipoLiteratura
    {
        public int Id { get; set; }
        public string? Descricao { get; set; }

        public TipoLiteratura()
        {
            Descricao = "N/D";
        }
    }
}
