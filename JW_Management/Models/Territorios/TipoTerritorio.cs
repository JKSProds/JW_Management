namespace JW_Management.Models
{
    public class TipoTerritorio
    {
        public string? Id { get; set; }
        public string? Descricao { get; set; }

        public TipoTerritorio()
        {
            Descricao = "N/D";
        }
    }
}
