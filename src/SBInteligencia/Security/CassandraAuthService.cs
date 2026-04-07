using System.Net;
using System.Net.Http.Headers;
using System.Text;
using SBInteligencia.DTO;
using System.Text;

namespace SBInteligencia.Security
{
    public class CassandraAuthService : IAuthService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public CassandraAuthService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _baseUrl = config["Cassandra:BaseUrl"];
        }

        public async Task<LoginResponse?> LoginAsync(string usuario, string password)
        {
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("username", usuario),
                new KeyValuePair<string, string>("password", password)
            });

            var response = await _http.PostAsync($"{_baseUrl}/cassandra/login", form);

            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("CASSANDRA LOGIN RESPONSE:");
            Console.WriteLine(content);

            if (!response.IsSuccessStatusCode)
                return null;

            // 🔥 EXTRAER COOKIE
            if (response.Headers.TryGetValues("Set-Cookie", out var cookies))
            {
                var sessionCookie = cookies
                    .FirstOrDefault(x => x.StartsWith("cassandra_session"));

                return new LoginResponse
                {
                    Usuario = usuario,
                    Token = sessionCookie // guardamos cookie completa
                };
            }

            return null;
        }



        public Task<UserSession?> GetUserInfo(string token)
        {
            try
            {
                // token = "cassandra_session=XXXXX; path=/..."
                var raw = token.Split(';')[0]; // cassandra_session=XXXXX
                var value = raw.Split('=')[1]; // XXXXX

                // separar BASE64 del resto (formato: base64.payload.signature)
                var base64Part = value.Split('.')[0];

                // decode Base64
                var jsonBytes = Base64UrlDecode(base64Part);
                var json = Encoding.UTF8.GetString(jsonBytes);

                Console.WriteLine("CASSANDRA COOKIE JSON:");
                Console.WriteLine(json);

                var data = System.Text.Json.JsonSerializer.Deserialize<SessionDto>(
                    json,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return Task.FromResult<UserSession?>(new UserSession
                {
                    Nombre = data?.Nombre ?? "",
                    Token = token,
                    Rol = data?.Rol ?? "",
                    Dependencia = data?.Destino ?? "" // 👈 fallback si querés
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR PARSEANDO COOKIE:");
                Console.WriteLine(ex.Message);
                return Task.FromResult<UserSession?>(null);
            }
        }
        private byte[] Base64UrlDecode(string input)
        {
            string output = input
                .Replace('-', '+')
                .Replace('_', '/');

            switch (output.Length % 4)
            {
                case 2: output += "=="; break;
                case 3: output += "="; break;
            }

            return Convert.FromBase64String(output);
        }
    }
}