using Newtonsoft.Json;

namespace OMMP.WebClient;

public class HttpHelper
{
    public static async Task<T> GetAsync<T>(string url)
    {
        using (var client = new HttpClient())
        {
            var uri = new Uri(new Uri(GlobalCache.Instance.CurrentClient.ClientApiUrl), url);
            var responseMessage = await client.GetAsync(uri);
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<T>(responseContent);
            return data;
        }
    }
}