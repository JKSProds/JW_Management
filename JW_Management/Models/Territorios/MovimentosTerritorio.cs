namespace JW_Management.Models
{
    public class MovimentosTerritorio
    {
        public string? Stamp { get; set; }
        public Territorio? Territorio { get; set; }
        public Publicador? Publicador { get; set; }
        public DateTime DataMovimento { get; set; }
        public TipoMovimentoTerritorio Tipo { get; set; }
    }

    public enum TipoMovimentoTerritorio
    {
        ENTRADA, SAIDA
    }
}
