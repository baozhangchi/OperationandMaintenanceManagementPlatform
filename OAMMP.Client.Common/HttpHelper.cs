using System.Text;
using Newtonsoft.Json;

namespace OAMMP.Client.Common;

public class HttpHelper
{
    public static async Task<T?> GetAsync<T>(string url)
    {
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync(url);
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseContent);
        }
    }

    public static async Task<T?> PostAsync<T>(string url, object data)
    {
        using (var client = new HttpClient())
        {
            var context = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, context);
            var responseContent = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseContent);
        }
    }
}