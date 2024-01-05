namespace JW_Management.Models
{
    public class TipoDesignacao
    {
        public int Id { get; set; }
        public string? Descricao { get; set; }
        public string? DescricaoAdicional { get; set; }
        public int? Salas { get; set; }
        public int NMin { get; set; }
    }
}
