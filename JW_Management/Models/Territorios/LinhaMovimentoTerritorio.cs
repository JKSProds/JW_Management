namespace JW_Management.Models
{
    public class RegistroTerritorio
    {
        public MovimentosTerritorio Entrada { get; set; }
        public MovimentosTerritorio Saida { get; set; }
        public MovimentosTerritorio UltimoMovimento { get { return string.IsNullOrEmpty(Saida.Stamp) ? Entrada : Saida; } }
        public string Descricao { get { return string.IsNullOrEmpty(this.Saida.Stamp) ? this.Entrada.DataDescritiva : this.Saida.DataDescritiva; } }
        public string? CorEstado { get { return UltimoMovimento.Tipo == TipoMovimentoTerritorio.ENTRADA ? ((DateTime.Now - UltimoMovimento.DataMovimento).Days > 120 ? "danger" : ((DateTime.Now - UltimoMovimento.DataMovimento).Days > 90 ? "warning" : "success")) : ((DateTime.Now - UltimoMovimento.DataMovimento).Days < 30 ? "grey" : ""); } }
        public RegistroTerritorio()
        {
            Entrada = new MovimentosTerritorio();
            Saida = new MovimentosTerritorio();
        }
    }


}
