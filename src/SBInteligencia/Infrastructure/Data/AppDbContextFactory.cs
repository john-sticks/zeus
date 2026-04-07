using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
            var connectionBase = _configuration.GetConnectionString("MySqlBase");

            var databaseName = $"delitos_{anio}";
            var connectionString = $"{connectionBase};Database={databaseName}";

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

            optionsBuilder.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 0, 36)), // 🔥 FIJO
            options =>
            {
                options.EnableRetryOnFailure();
            });

            return new AppDbContext(optionsBuilder.Options);
        }
        public SBInteligenciaDbContext CreateAnalytics()
        {
            var connectionString = _configuration.GetConnectionString("MySqlAnalytics");

            var optionsBuilder = new DbContextOptionsBuilder<SBInteligenciaDbContext>();

            optionsBuilder.UseMySql(
            connectionString,
            new MySqlServerVersion(new Version(8, 0, 36)), // 🔥 FIJO
            options =>
            {
                options.EnableRetryOnFailure();
            });

            return new SBInteligenciaDbContext(optionsBuilder.Options);
        }
    }
}