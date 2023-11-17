using System.Text;
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

    public static async Task<T> PostAsync<T>(string url, object data)
    {
        using (var client = new HttpClient())
        {
            var uri = new Uri(new Uri(GlobalCache.Instance.CurrentClient.ClientApiUrl), url);
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var responseMessage = await client.PostAsync(uri, content);
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseContent);
        }
    }

    public static async Task<T> PutAsync<T>(string url, object data)
    {
        using (var client = new HttpClient())
        {
            var uri = new Uri(new Uri(GlobalCache.Instance.CurrentClient.ClientApiUrl), url);
            var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
            var responseMessage = await client.PutAsync(uri, content);
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseContent);
        }
    }

    public static async Task<T> SendAsync<T>(string url, object data = null, HttpMethod method = null)
    {
        using (var client = new HttpClient())
        {
            var uri = new Uri(new Uri(GlobalCache.Instance.CurrentClient.ClientApiUrl), url);
            var requestMessage = new HttpRequestMessage(method ?? HttpMethod.Get, uri);
            if (data != null)
            {
                var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                requestMessage.Content = content;
            }

            var responseMessage = await client.SendAsync(requestMessage);
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseContent);
        }
    }

    public static async Task<T> DeleteAsync<T>(string url)
    {
        using (var client = new HttpClient())
        {
            var uri = new Uri(new Uri(GlobalCache.Instance.CurrentClient.ClientApiUrl), url);
            var responseMessage = await client.DeleteAsync(uri);
            var responseContent = await responseMessage.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(responseContent);
        }
    }
}