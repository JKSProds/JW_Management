namespace JW_Management.Models
{
    public class Reuniao
    {
        public string? Stamp { get; set; }
        public string? SemanaReuniao { get; set; }
        public List<Designacao>? Designacoes {get; set;}
        public List<Cantico> Canticos { get; set; }
        public List<Grupo> Grupos { get; set; }
        
        public List<Designacao> ObterDesignacoesTipo(int tipo) {
            return this.Designacoes.Where(t => t.TipoDesignacao!.Id > tipo).Where(t => t.TipoDesignacao!.Id < tipo+100).DistinctBy(t => t.Distinct).OrderBy(t => t.TipoDesignacao!.Id).ToList();
        }
    }
}
