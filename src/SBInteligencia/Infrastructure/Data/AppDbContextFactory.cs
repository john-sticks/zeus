using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using SBInteligencia.Entities;
using SBInteligencia.Entities.Analytics;

namespace SBInteligencia.Infrastructure.Data
{
    public interface IAppDbContextFactory
    {
        AppDbContext Create(int anio);
        SBInteligenciaDbContext CreateAnalytics();
    }

    public class AppDbContextFactory : IAppDbContextFactory
    {
        private readonly IConfiguration _configuration;

        public AppDbContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public AppDbContext Create(int anio)
        {
            var baseConn = _configuration.GetConnectionString("MySqlBase");

            if (string.IsNullOrEmpty(baseConn))
                throw new Exception("Connection string MySqlBase no configurada");

            var builder = new MySqlConnectionStringBuilder(baseConn)
            {
                Database = $"delitos_{anio}"
            };

            var connectionString = builder.ConnectionString;

            // 🔍 DEBUG (podés dejarlo temporalmente)
            Console.WriteLine($"[DB] Conectando a: {builder.Server} / {builder.Database}");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 36)),
                options =>
                {
                    options.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null
                    );
                });

            return new AppDbContext(optionsBuilder.Options);
        }

        public SBInteligenciaDbContext CreateAnalytics()
        {
            var connectionString = _configuration.GetConnectionString("MySqlAnalytics");

            if (string.IsNullOrEmpty(connectionString))
                throw new Exception("Connection string MySqlAnalytics no configurada");

            var optionsBuilder = new DbContextOptionsBuilder<SBInteligenciaDbContext>();

            optionsBuilder.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 36)),
                options =>
                {
                    options.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorNumbersToAdd: null
                    );
                });

            return new SBInteligenciaDbContext(optionsBuilder.Options);
        }
    }
}