using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace SBInteligencia.Security
{
    public class AuthService
    {
        private readonly HttpClient _http;
        private readonly string _baseUrl;

        public AuthService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _baseUrl = config["cerberus:BaseUrl"];
        }

        // 🔹 NUEVO → Login contra Cerberus (todavía no lo usamos)
        public async Task<LoginResponse?> LoginAsync(string usuario, string password)
        {
            var form = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("username", usuario),
                new KeyValuePair<string, string>("password", password),
                new KeyValuePair<string, string>("client_id", "sbinteligencia") // 👈 agregar
            });

            var request = new HttpRequestMessage(HttpMethod.Post,
                $"{_baseUrl}/cerberus/api/v1/auth/token");

            request.Content = form;

            request.Headers.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _http.SendAsync(request);

            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("CERBERUS RESPONSE:");
            Console.WriteLine(content);

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("ERROR STATUS: " + response.StatusCode);
                Console.WriteLine("ERROR BODY:");
                Console.WriteLine(content);
                return null;
            }

            var result = System.Text.Json.JsonSerializer.Deserialize<TokenResponse>(
                content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return new LoginResponse
            {
                Usuario = usuario,
                Token = result?.access_token
            };
        }        // 🔹 MANTENEMOS lo que ya tenías (cookies)
        public async Task SignIn(HttpContext context, UserSession user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Nombre),
        new Claim("Token", user.Token)
    };

            var identity = new ClaimsIdentity(claims, "CookieAuth");
            var principal = new ClaimsPrincipal(identity);

            await context.SignInAsync("CookieAuth", principal);

            // 🔥 CLAVE: guardar en sesión también
            context.Session.SetString("token", user.Token);
        }
        public async Task SignOut(HttpContext context)
        {
            await context.SignOutAsync("CookieAuth");
        }
    }

    // 🔹 Modelo base (temporal)
    public class LoginResponse
    {
        public string Token { get; set; }
        public string Usuario { get; set; }
    }
}