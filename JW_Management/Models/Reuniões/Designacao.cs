namespace JW_Management.Models
{
    public class Designacao
    {
        public string? Stamp { get; set; }
        public string? SemanaReuniao { get; set; }
        public string? NomeDesignacao { get; set; }
        public string? NomePublicador { get; set; }
        public TipoDesignacao? TipoDesignacao { get; set; }
        public Publicador? Publicador { get; set; }
        public string? Local { get; set; }
        public string Distinct { get {return Local + "_" + TipoDesignacao.Id.ToString();} }

        public Designacao() {
            Publicador = new Publicador() {Id=0};
            TipoDesignacao = new TipoDesignacao() {Id=0};
        }
    }
}
