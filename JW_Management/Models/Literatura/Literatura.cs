using System.Globalization;

namespace JW_Management.Models
{
    public class Literatura
    {
        public string? Stamp { get; set; }
        public int Id { get; set; }
        public string? Referencia { get; set; }
        public string? Data { get; set; }
        public DateTime? DataRevista => DateTime.TryParseExact(Data ?? "", new[] { "yyyyMM", "yyyy" }, new CultureInfo("pt-PT"), DateTimeStyles.None, out var dt) ? (Data.Length == 4 ? new DateTime(dt.Year, 1, 1) : dt) : (DateTime?)null;
        public string? Imagem { get; set; }
        public string? Descricao { get; set; }
        public int Quantidade { get; set; }
        public int QuantidadeFalta { get; set; }
        public int Entradas { get; set; }
        public int Saidas { get; set; }
        public int Linha { get; set; }
        public string? DescricaoGeral { get; set; }
        public TipoLiteratura? Tipo { get; set; }
        public Publicador? Publicador { get; set; }
        public EstadoPedido? EstadoPedido { get; set; }
        public string URL => GetUrl();
        
        public int Entrega => ((Tipo is { Id: 7 } && DataRevista != null ? (Referencia == "w" || Referencia == "wlp" ? DataRevista.Value.AddMonths(2) : DataRevista.Value) : DateTime.Now) - DateTime.Now).Days;

        private string GetUrl() {
            string baseURL = "https://assetsnffrgf-a.akamaihd.net/assets/a/";
            string baseURL2 = "https://cms-imgp.jw-cdn.org/img/p/";
            
            string res = "";
            
            if (Tipo != null && Tipo.Id == 7) {
                if (Referencia == "w" || Referencia == "wlp") {
                    res =  "w/" + this.Data +  "/TPO/pt/w_TPO_" + Data + "_lg.jpg";
                    return baseURL2 + res;  
                }if (Referencia == "es" || Referencia == "eslp") {
                     res = "es"+Data[^2..]+"/TPO/wpub/es"+Data[^2..]+"_TPO_lg.jpg";
                }else {
                    res = Referencia + "/" + Data +  "/TPO/pt/"  + Referencia + "_TPO_" + Data + "_lg.jpg";
                    return baseURL2 + res;
                }
            }else {
                res = Referencia + "/TPO/wpub/" + Referencia + "_TPO_lg.jpg";
            }

           return baseURL + res;
        }
    }
}
