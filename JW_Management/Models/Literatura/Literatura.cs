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

        public string GetUrl() {
            string baseURL = "https://assetsnffrgf-a.akamaihd.net/assets/a/";
            string res = "";
            string responseBody = "";

            if (this.Tipo.Id == 7) {
                if (this.Referencia == "w" || this.Referencia == "wlp") {
                    res = "w/TPO/" + this.Data + "/wpub/w_TPO_" + this.Data + "_lg.jpg";
                }else if (this.Referencia == "es" || this.Referencia == "eslp") {
                     res = "es"+this.Data+"/TPO/wpub/es"+Data+"_TPO_lg.jpg";
                }else {
                    res = this.Referencia + "/TPO/" + this.Data + "/wpub/" + this.Referencia + "_TPO_" + this.Data + "_lg.jpg";
                }
            }else {
                res = this.Referencia + "/TPO/wpub/" + this.Referencia + "_TPO_lg.jpg";
            }

            var httpClientHandler = new HttpClientHandler();
            var httpClient = new HttpClient(httpClientHandler)
            {
            BaseAddress = new Uri(baseURL + res)
            };
            using (var response = httpClient.GetAsync(baseURL + res))
            {
            responseBody = response.Result.Content.ReadAsStringAsync().Result;
            }

            return responseBody.Length < 20 ? this.Imagem : baseURL + res;
        }
    }
}
