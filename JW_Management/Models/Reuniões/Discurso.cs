namespace JW_Management.Models
{
    public class Discurso
    {
        public string? Stamp { get; set; }
        public DateTime DataDiscurso { get; set; }
        public Publicador? Pub { get; set; }
        public string? NomePublicador { get ; set; }
        public string? Congregacao { get ; set; }
        public int NumDiscurso { get; set; }
        public string? TemaDiscurso { get; set; }
        public bool OradorExterno { get {return this.Pub.Id==0;}}
    }
}
