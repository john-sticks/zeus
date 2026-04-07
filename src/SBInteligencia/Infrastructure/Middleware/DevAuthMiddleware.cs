using System.Security.Claims;

namespace SBInteligencia.Infrastructure.Middleware
{
    public class DevAuthMiddleware
    {
        private readonly RequestDelegate _next;

        public DevAuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var env = context.RequestServices
    .GetRequiredService<IWebHostEnvironment>();
            if (!context.User.Identity.IsAuthenticated)
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, $"DEV USER ({env.EnvironmentName})"),
                new Claim(ClaimTypes.Role, "ADMIN"),
                new Claim("Dependencia", "SISTEMAS")
            };

                var identity = new ClaimsIdentity(claims, "Cookies");
                context.User = new ClaimsPrincipal(identity);
            }

            await _next(context);
        }
    }
}
