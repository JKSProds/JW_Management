using System.Text;
using System.Text.RegularExpressions;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;

namespace JW_Management.Models;

public class JWApiContext
{
    //API CALLS
    public AppContext _appContext { get; set; }
    private string CongId => _appContext._currentTenant.JWApiIdCongregacao;
    public string Cookies {get;set;} 
    private string XSRF_Token => Regex.Match(Cookies, @"XSRF-TOKEN=([^;]+)").Groups[1].Value;
    private string BaseURL => $"https://hub.jw.org/congregation-literature/api";

    public JWApiContext(AppContext appContext)
    {
        _appContext = appContext;
    }
    
    private string GET(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            // Adicione o cabeçalho do cookie
            client.DefaultRequestHeaders.Add("Cookie", Cookies);

            try
            {
                // Realize a chamada síncrona
                HttpResponseMessage response = client.GetAsync(url).Result;

                if (response.IsSuccessStatusCode)
                {
                    // Leia o conteúdo da resposta como string
                    string responseData = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine($"[GET] - ({url}): {responseData})");
                    return responseData;
                }
                else
                {
                    Console.WriteLine($"Erro: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao fazer a solicitação: {ex.Message}");
            }
        }

        return string.Empty;
    }
    
    private string PUT(string url, string body)
    {
        
        using (HttpClient client = new HttpClient())
        {

            // Adicione o cabeçalho do cookie
            client.DefaultRequestHeaders.Add("Cookie", Cookies);
            client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", XSRF_Token);
            
            // Criar o conteúdo da requisição
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            try
            {
                // Enviar requisição PUT
                HttpResponseMessage response = client.PutAsync(url, content).Result;

                // Verificar a resposta
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[PUT] - ({url}) - ({body}): {response.Content.ReadAsStringAsync().Result})");
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    Console.WriteLine($"Erro: {response.StatusCode}");
                    Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar a requisição: {ex.Message}");
            }
        }
        return string.Empty;
    }
    
    private string POST(string url, string body)
    {
        
        using (HttpClient client = new HttpClient())
        {

            // Adicione o cabeçalho do cookie
            client.DefaultRequestHeaders.Add("Cookie", Cookies);
            client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", XSRF_Token);
            
            // Criar o conteúdo da requisição
            var content = new StringContent(body, Encoding.UTF8, "application/json");

            try
            {
                // Enviar requisição PUT
                HttpResponseMessage response = client.PostAsync(url, content).Result;

                // Verificar a resposta
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[POST] - ({url}) - ({body}): {response.Content.ReadAsStringAsync().Result})");
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    Console.WriteLine($"Erro: {response.StatusCode}");
                    Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao enviar a requisição: {ex.Message}");
            }
        }
        return string.Empty;
    }
    
    public Inventario ObterInventario()
    {
        Inventario i = new Inventario();

        try
        {
            // Parse o JSON em um JObject
            JObject jsonObject = JObject.Parse(GET($"{BaseURL}/inventory-reports/{CongId}/summaries"));
        
            // Obtenha o primeiro elemento da lista "reportSummaries"
            var reportSummary = jsonObject["reportSummaries"]?[0];

            if (reportSummary != null)
            {
                i.StampInventario = reportSummary.Value<string>("languageGuid") ?? "";
                i.DataLimite = reportSummary.Value<DateTime>("dueDate");
                i.DataInventario= reportSummary.Value<DateTime>("reportDate");
                i.Literatura = ObterLiteraturas(i.StampInventario);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        return i;
    }

    public List<Literatura> ObterLiteraturas(string StampInventario)
    {
        List<Literatura> l = new List<Literatura>();

        try
        {
            // Parse o JSON em um JObject
            JObject jsonObject = JObject.Parse(GET($"{BaseURL}/inventory-reports/{CongId}/{StampInventario}/catalog"));
        
            // Obtenha o primeiro elemento da lista "reportSummaries"
            var categories = jsonObject["categories"];

            foreach (var category in categories)
            {
                var items = category["items"];
            
                foreach (var item in items)
                {
                    l.Add(new Literatura()
                    {
                        Stamp = item.Value<string>("catalogItemGuid"),
                        Descricao = item.Value<string>("description"),
                        Referencia = item.Value<string>("symbol") ?? item.Value<string>("mnemonicCode"),
                    });
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        
        return l;
    }
    
    public string AtualizarLiteratura(Inventario i, Literatura l)
    {
        try
        {
            var jsonData = $"[{{\"catalogItemGuid\":\"{l.Stamp}\",\"currentQuantity\":{l.Quantidade},\"isDone\":true}}]";
        
            return PUT($"{BaseURL}/inventory-reports/{CongId}/{i.StampInventario}/save-items", jsonData);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return string.Empty;

    }
    
    public string FecharInventario(Inventario i)
    {
        try
        {
            return POST($"{BaseURL}/inventory-reports/{CongId}/{i.StampInventario}/submit-report", "{}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return string.Empty;
    }
    
    public List<Literatura> ObterPeriodicos(string ano, Literatura tLiteratura)
    {
        List<Literatura> LstLiteratura = new List<Literatura>();
        try
        {
            for (int i = 1; i <= 12; i++)
            {
                Literatura l = new Literatura() {Data=$"{ano}{i:D2}", Referencia = tLiteratura.Referencia, Descricao = $"{tLiteratura.Descricao} - Nº{i} ({ano})", Tipo = new TipoLiteratura() {Id = 7}};
                using HttpClient client = new HttpClient();
                
                var response = client.GetAsync(l.GetUrl()).Result;

                if (response.IsSuccessStatusCode)
                {
                    LstLiteratura.Add(l);
                }
                
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        
        return LstLiteratura;
    }
}