namespace JW_Management.Models
{
    public class MovimentosTerritorio
    {
        public string? Stamp { get; set; }
        public Territorio? Territorio { get; set; }
        public Publicador? Publicador { get; set; }
        public DateTime DataMovimento { get; set; }
        public TipoMovimentoTerritorio Tipo { get; set; }
        public string DataDescritiva { get { return string.IsNullOrEmpty(this.Stamp) ? "" : (this.Tipo == TipoMovimentoTerritorio.ENTRADA ? "Entregue à " : "Devolvido à ") + ((int)(DateTime.Now - this.DataMovimento).TotalDays).ToString() + " dia(s)"; } }

        public MovimentosTerritorio()
        {
            Tipo = TipoMovimentoTerritorio.SAIDA;
        }
    }

    public enum TipoMovimentoTerritorio
    {
        ENTRADA, SAIDA
    }
}
