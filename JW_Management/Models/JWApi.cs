using System.Text;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;

namespace JW_Management.Models;

public class JWApi
{
    //API CALLS

    private string CongId => "07ec6967-d680-4f17-ba8c-1ffc08949dcb";
    private string Token =>
        $"XSRF-TOKEN={XSRFToken}; cdh.session.expiry=1734014321; .AspNetCore.Cookies=CfDJ8BE7W9YBmqBFsmsJNwHOyGUxSNK-6J5KeXDKW7Cxv0zLDFYKg88sPabADrV5Yg1pn9P44NFTviw9Cjwir6FOSPzUn-z9LuiYvdYPsnUFykozzySa2jEaSYbbOz6p8ceBYIm1_B3IKdyXxWbHDQOCG-X2Jtxi_9R1l77zK1LH3QIPjYHrvqgLZL9LJlUBcYicduXDGokDS51TOXM61g9D962mpwGZ3N19fK5yD73THadiZYsqiJFK_ol_azyjvqvBB7usm59cfWKSTmW8brblAJoFni4Pq6LXTSKeN0LhlRYO; tenant=jworg; .AspNetCore.Antiforgery.9L1Lz1Gp5HM=CfDJ8BE7W9YBmqBFsmsJNwHOyGX6mCXOl-D2bhhuuj1UcyT0rHq1dG1A3YuBjy4gp4qoNgJ4aZAjl8BaHWPC9ryxyl_8qWiWyLqdAeuXI1qgxVDQC8zCozgDKJc5EirKjVe4s14s7kivQTuvmEayIgoi_og; ptrn.lang=pt-pt; cookieConsent-DIAGNOSTIC=true; cookieConsent-FUNCTIONAL=true; cookieConsent-STRICTLY_NECESSARY=true; cookieConsent-USAGE=true";
    public string XSRFToken { get; set; } 
    private string BaseURL => $"https://hub.jw.org/congregation-literature/api";

    private string GET(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            // Adicione o cabeçalho do cookie
            client.DefaultRequestHeaders.Add("Cookie", Token);

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
            client.DefaultRequestHeaders.Add("Cookie", Token);
            client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", XSRFToken);
            
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
            client.DefaultRequestHeaders.Add("Cookie", Token);
            client.DefaultRequestHeaders.Add("X-XSRF-TOKEN", XSRFToken);
            
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
        
        return i;
    }

    public List<Literatura> ObterLiteraturas(string StampInventario)
    {
        List<Literatura> l = new List<Literatura>();
        
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
        
        return l;
    }
    
    public string AtualizarLiteratura(Inventario i, Literatura l)
    {
        var jsonData = $"[{{\"catalogItemGuid\":\"{l.Stamp}\",\"currentQuantity\":{l.Quantidade},\"isDone\":true}}]";
        
        return PUT($"{BaseURL}/inventory-reports/{CongId}/{i.StampInventario}/save-items", jsonData);
    }
    
    public string FecharInventario(Inventario i)
    {
        return POST($"{BaseURL}/inventory-reports/{CongId}/{i.StampInventario}/submit-report", "{}");
    }
}