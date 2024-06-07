using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace drc.Services
{
    public class HateSpeechService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly string _apiHost;

        public HateSpeechService(HttpClient httpClient, IOptions<ApiSettings> apiSettings)
        {
            _httpClient = httpClient;
            _apiKey = apiSettings.Value.ApiKey ?? throw new ArgumentNullException(nameof(apiSettings.Value.ApiKey));
            _apiHost = apiSettings.Value.ApiHost ?? throw new ArgumentNullException(nameof(apiSettings.Value.ApiHost));
        }

        public async Task<string> DetectHateSpeechAsync(string text)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://{_apiHost}/process");
            request.Headers.Add("x-rapidapi-key", _apiKey);
            request.Headers.Add("x-rapidapi-host", _apiHost);
            request.Content = new StringContent(JsonConvert.SerializeObject(new { text }), Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
    }
}
