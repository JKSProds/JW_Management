namespace JW_Management.Models
{
    public class Recomendacao
    {
        public string Id { get; set; }
        public List<Recomendacao_Linhas> Linhas { get; set; } = new List<Recomendacao_Linhas>();
        public Congresso Congresso { get; set; }
        public int IdCongregacao { get; set; }
        public int Sequencia  { get; set; }

        public string Paste => string.Join(" + ", Linhas.Select(o => o.TipoTransporte.Nome)) + "\t" + 
                               string.Join(" | ", Linhas.Select(o =>
                o.Rota.Nome.ToUpper() + " (" + o.Viagem_Paragem.Paragem.NomeParagem + " -> " +
                o.Viagem_Paragem.Viagem.Destino + ")")) + "\t" + 
                               string.Join(" + ", Linhas.Select(o =>
                    o.Viagem_Paragem.DataPartida == DateTime.MinValue
                        ? "TBD"
                        : o.Viagem_Paragem.DataPartida.ToString("HH:mm")));
    }
    
    public class Recomendacao_Linhas
    {
        public string IdRecomendacao { get; set; }
        public string Id { get; set; }
        public TipoTransporte TipoTransporte { get; set; }
        public Rota Rota { get; set; }
        public Viagem_Paragem Viagem_Paragem { get; set; }
        public bool Manual { get; set; }
        public int Sequencia  { get; set; }
    }
}