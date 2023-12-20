using System.Reflection.Metadata.Ecma335;

namespace JW_Management.Models
{
    public class Territorio
    {
        public string? Stamp { get; set; }
        public string? Id { get; set; }
        public string? Nome { get; set; }
        public string? CorEstado { get { return UltimoMovimento.Tipo == TipoMovimentoTerritorio.ENTRADA ? ((DateTime.Now - UltimoMovimento.DataMovimento).Days > 120 ? "danger" : ((DateTime.Now - UltimoMovimento.DataMovimento).Days > 90 ? "warning" : "success")) : ((DateTime.Now - UltimoMovimento.DataMovimento).Days < 30 ? "grey" : ""); } }
        public DificuldadeTerritorio Dificuldade { get; set; }
        public TipoTerritorio Tipo { get; set; }
        public string? Descricao { get; set; }
        public string? Url { get; set; }
        public List<MovimentosTerritorio>? Movimentos { get; set; }
        public List<RegistroTerritorio>? Registros { get; set; }
        public List<AnexosTerritorio>? Anexos { get; set; }
        public MovimentosTerritorio? UltimoMovimento { get; set; }
        public string CorDificuldade { get{return Dificuldade==DificuldadeTerritorio.FACIL ? "#008000" : Dificuldade==DificuldadeTerritorio.MODERADO ? "#FFA500" : "#ff0000";} }
    }

    public enum DificuldadeTerritorio
    {
        FACIL, MODERADO, DIFICIL
    }
}
