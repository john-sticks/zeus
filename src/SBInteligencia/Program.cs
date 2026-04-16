using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using SBInteligencia.Entities;
using SBInteligencia.Infrastructure.Data;
using SBInteligencia.Infrastructure.Middleware;
using SBInteligencia.Security;
using SBInteligencia.Services;
using System.Net;

namespace SBInteligencia
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            // 🔥 FORZAR ORDEN
            builder.Configuration
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables(); // 👈 CLAVE
            // 🔹 MVC + JSON
            builder.Services.AddControllersWithViews()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy =
                        System.Text.Json.JsonNamingPolicy.CamelCase;
                });

            builder.Services.AddHttpContextAccessor();

            // 🔹 AUTH
            var authMode = builder.Configuration["Auth:Mode"];
            var appMode = builder.Configuration["App:Mode"];

            Console.WriteLine("ENV: " + builder.Environment.EnvironmentName);
            Console.WriteLine("AUTH MODE: " + authMode);
            Console.WriteLine("APP MODE: " + appMode);

            if (authMode == "Cerberus")
            {
                var baseUrl = builder.Configuration["Cerberus:BaseUrl"];
                var apiKey = builder.Configuration["Cerberus:ServiceApiKey"];

                if (string.IsNullOrEmpty(baseUrl) || string.IsNullOrEmpty(apiKey))
                    throw new Exception("🚨 Cerberus mal configurado");
            }

            switch (authMode)
            {
                case "Mock":
                    builder.Services.AddScoped<IAuthService, MockAuthService>();
                    break;

                case "Cassandra":
                    builder.Services.AddHttpClient<IAuthService, CassandraAuthService>()
                        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                        {
                            UseCookies = true,
                            CookieContainer = new CookieContainer()
                        });
                    break;

                case "Cerberus":
                default:
                    builder.Services.AddHttpClient<IAuthService, CerberusAuthService>();
                    break;
            }

            builder.Services.AddScoped<AuthCookieService>();

            builder.Services.AddAuthentication("Cookies")
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = "/Login";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                });

            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            // 🔹 SESSION
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // 🔹 DB CONTEXT (UNIFICADO SBInteligencia)
            builder.Services.AddDbContext<SBInteligenciaDbContext>(options =>
                options.UseMySql(
                    builder.Configuration.GetConnectionString("MySqlAnalytics"),
                    new MySqlServerVersion(new Version(8, 0, 36))
                ));

            // 🔹 FACTORY (delitos por año)
            builder.Services.AddScoped<IAppDbContextFactory, AppDbContextFactory>();

            // 🔹 SERVICES
            builder.Services.AddScoped<MenuXmlService>();
            builder.Services.AddScoped<CoberturaService>();
            builder.Services.AddScoped<HechoService>();
            builder.Services.AddScoped<InformeService>();
            builder.Services.AddScoped<DashboardService>();
            Console.WriteLine("MySqlBase: " + builder.Configuration.GetConnectionString("MySqlBase"));
            var app = builder.Build();

            // 🔹 STATIC FILES
            var provider = new FileExtensionContentTypeProvider();
            provider.Mappings[".geojson"] = "application/json";

            app.UseHttpsRedirection();

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = provider
            });

            app.UseRouting();

            // 🔹 MIDDLEWARES
            app.UseMiddleware<ErrorMiddleware>();

            app.UseSession();

            app.UseAuthentication();
            //if (app.Environment.IsDevelopment())
            //{
            //    app.UseMiddleware<DevAuthMiddleware>(); // 🔥 SOLO DEV
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //    app.UseHsts();
            //}
            app.UseAuthorization();
            app.UseMiddleware<MenuAuthorizationMiddleware>();
            // 🔹 ENDPOINTS
            app.MapControllers();

            app.MapGet("/debug/routes", (IEnumerable<EndpointDataSource> sources) =>
            {
                return sources
                    .SelectMany(s => s.Endpoints)
                    .Select(e => e.DisplayName);
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Login}/{action=Index}/{id?}");
            await app.RunAsync();
        }
    }
}


