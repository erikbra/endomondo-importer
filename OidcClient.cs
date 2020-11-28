using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace endoimport
{
    public class OidcClient
    {
        private readonly HttpClient _httpClient;
        private readonly StravaAppConfig _config;
        
        public OidcClient(HttpClient httpClient, IOptions<StravaAppConfig> config)
        {
            _httpClient = httpClient;
            _config = config.Value;
        }

        public async Task<OidcResponse> Login(string code)
        {
            var body = new Dictionary<string, string>
            {
                {"client_id", _config.ClientId},
                {"client_secret", _config.ClientSecret},
                {"code", code},
                {"grant_type", "authorization_code"}
            };

            return await SendIssuerRequest(body);
        }

        public async Task<OidcResponse> Refresh(string refreshToken)
        {
            IDictionary<string, string> body = new Dictionary<string, string>
            {
                {"client_id", _config.ClientId},
                {"client_secret", _config.ClientSecret},
                {"refresh_token", refreshToken},
                {"grant_type", "refresh_token"}
            };

            return await SendIssuerRequest(body);
        }

        private async Task<OidcResponse> SendIssuerRequest(IEnumerable<KeyValuePair<string, string>> data)
        {
            var message = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("/oauth/token", UriKind.Relative),
                Content = new FormUrlEncodedContent(data)
            };

            var response = await _httpClient.SendAsync(message);

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<OidcResponse>();
        }
    }
}