namespace JW_Management.Models
{
    public class Literatura
    {
        public string? Stamp { get; set; }
        public int Id { get; set; }
        public string? Referencia { get; set; }
        public string? Data { get; set; }
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
        public string URL => GetUrl();

        public string GetUrl() {
            string baseURL = "https://assetsnffrgf-a.akamaihd.net/assets/a/";
            string baseURL2 = "https://cms-imgp.jw-cdn.org/img/p/";
            
            string res = "";
            
            if (Tipo != null && Tipo.Id == 7) {
                if (this.Referencia == "w" || this.Referencia == "wlp") {
                    res =  "w/" + this.Data +  "/TPO/pt/w_TPO_" + Data + "_lg.jpg";
                    return baseURL2 + res;                
                }if (this.Referencia == "mvpwp" ) {
                    res = "wp/"+this.Data+"/TPO/pt/wp_TPO_"+Data+"_lg.jpg";
                    return baseURL2 + res;
                }if (this.Referencia == "mvpg" ) {
                    res = "g/"+this.Data+"/TPO/pt/g_TPO_"+Data+"_lg.jpg";
                    return baseURL2 + res;
                }if (this.Referencia == "es" || this.Referencia == "eslp") {
                     res = "es"+this.Data+"/TPO/wpub/es"+Data+"_TPO_lg.jpg";
                }else {
                    res = this.Referencia + "/" + this.Data +  "/TPO/pt/"  + this.Referencia + "_TPO_" + this.Data + "_lg.jpg";
                    return baseURL2 + res;
                }
            }else {
                res = this.Referencia + "/TPO/wpub/" + this.Referencia + "_TPO_lg.jpg";
            }

           return baseURL + res;
        }
    }
}
