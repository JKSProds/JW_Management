namespace JW_Management.Models
{
    public class Literatura
    {
        public string? Stamp { get; set; }
        public int Id { get; set; }
        public string? Referencia { get; set; }
        public string? Imagem { get; set; }
        public string? Descricao { get; set; }
        public int Quantidade { get; set; }
        public int Entradas { get; set; }
        public int Saidas { get; set; }
        public int Linha { get; set; }
        public string? DescricaoGeral { get; set; }
        public TipoLiteratura? Tipo { get; set; }
        public Publicador? Publicador { get; set; }
        public EstadoPedido? EstadoPedido { get; set; }
    }
}
