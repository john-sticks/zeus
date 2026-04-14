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
                Database = $"delitos_{anio}",

                // 🔥 CLAVE: evitar bloqueos en container
                ConnectionTimeout = 10,
                DefaultCommandTimeout = 30,
                Pooling = true,
                MinimumPoolSize = 0,
                MaximumPoolSize = 10,
                ConnectionReset = false
            };

            var connectionString = builder.ConnectionString;

            Console.WriteLine($"[DB] Conectando a: {builder.Server} / {builder.Database}");

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 36))
            // ❌ IMPORTANTE: SIN EnableRetryOnFailure
            );

            // 🔥 LOG (opcional pero recomendable ahora)
            optionsBuilder
                .LogTo(Console.WriteLine)
                .EnableSensitiveDataLogging();

            return new AppDbContext(optionsBuilder.Options);
        }

        public SBInteligenciaDbContext CreateAnalytics()
        {
            var baseConn = _configuration.GetConnectionString("MySqlAnalytics");

            if (string.IsNullOrEmpty(baseConn))
                throw new Exception("Connection string MySqlAnalytics no configurada");

            var builder = new MySqlConnectionStringBuilder(baseConn)
            {
                ConnectionTimeout = 10,
                DefaultCommandTimeout = 30,
                Pooling = true,
                MinimumPoolSize = 0,
                MaximumPoolSize = 10,
                ConnectionReset = false
            };

            var connectionString = builder.ConnectionString;

            var optionsBuilder = new DbContextOptionsBuilder<SBInteligenciaDbContext>();

            optionsBuilder.UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 36))
            // ❌ SIN retry también
            );

            optionsBuilder
                .LogTo(Console.WriteLine)
                .EnableSensitiveDataLogging();

            return new SBInteligenciaDbContext(optionsBuilder.Options);
        }
    }
}