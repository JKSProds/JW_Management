namespace JW_Management.Models
{
    public class Territorio
    {
        public string? Stamp { get; set; }
        public string? Id { get; set; }
        public string? Nome { get; set; }
        public DificuldadeTerritorio Dificuldade { get; set; }
        public string? Descricao { get; set; }
        public string? Url { get; set; }
        public List<MovimentosTerritorio>? Movimentos { get; set; }
        public List<AnexosTerritorio>? Anexos { get; set; }
        public MovimentosTerritorio? UltimoMovimento { get; set; }
    }

    public enum DificuldadeTerritorio
    {
        FACIL, MODERADO, DIFICIL
    }
}
