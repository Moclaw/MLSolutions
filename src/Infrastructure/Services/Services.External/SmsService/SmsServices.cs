using Core.Configurations;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Services.External.SmsService
{
    public class SmsServices(IOptions<SmsConfiguration> options) : ISmsServices
    {
        private readonly string _apiKey = options.Value.ApiKey;
        private readonly string _apiSecret = options.Value.ApiSecret;
        private readonly string _senderId = options.Value.SenderId;
        private readonly string _baseUrl = options.Value.BaseUrl;
        private readonly int _timeout = options.Value.Timeout;
        private readonly bool _useDefaultCredentials = options.Value.UseDefaultCredentials;

        public void SendSms(string to, string message)
        {
            SendSms([to], message);
        }

        public void SendSms(List<string> to, string message)
        {
            using var httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(_timeout)
            };

            if (_useDefaultCredentials)
            {
                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_apiKey}:{_apiSecret}")));
            }

            var payload = new
            {
                sender = _senderId,
                recipients = to,
                text = message
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var response = httpClient.PostAsync(_baseUrl, content).GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
            {
                var error = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                throw new InvalidOperationException($"Failed to send SMS: {response.StatusCode} - {error}");
            }
        }
    }
}
