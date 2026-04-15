using System.Net.Http.Headers;
using System.Text.Json;

namespace SBInteligencia.Security
{
    public class CerberusAuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public CerberusAuthService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _baseUrl = config["Cerberus:BaseUrl"];
            _apiKey = config["Cerberus:ServiceApiKey"];
        }

        public async Task<LoginResponse?> LoginAsync(string usuario, string password)
        {
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", usuario),
                new KeyValuePair<string, string>("password", password)
            });

            var response = await _http.PostAsync(
                $"{_baseUrl}/cerberus/api/v1/auth/token",
                form);

            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("CERBERUS RESPONSE:");
            Console.WriteLine(content);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = JsonSerializer.Deserialize<TokenResponse>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new LoginResponse
            {
                Usuario = usuario,
                Token = result?.access_token
            };
        }

        public async Task<UserSession?> GetUserInfo(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get,
                $"{_baseUrl}/cerberus/api/v1/users/info-servicio");

            request.Headers.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            request.Headers.Add("Service-Api-Key", _apiKey);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _http.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("USER INFO:");
            Console.WriteLine(content);

            if (!response.IsSuccessStatusCode)
                return null;

            var data = JsonSerializer.Deserialize<CerberusUserResponse>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new UserSession
            {
                Nombre = $"{data?.nombre} {data?.apellido}",
                Token = token,
                Rol = data?.rol ?? "",
                Dependencia = data?.destino ?? ""
            };
        }
    }
    public class CerberusUserResponse
    {
        public string nombre { get; set; }
        public string apellido { get; set; }
        public string rol { get; set; }
        public string destino { get; set; }
    }
}