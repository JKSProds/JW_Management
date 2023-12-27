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
        public Local? Local { get; set; }

        public Designacao() {
            Publicador = new Publicador() {Id=0};
            TipoDesignacao = new TipoDesignacao() {Id=0};
        }
    }

    public enum Local {
        Auditorio, Sala
     }
}
