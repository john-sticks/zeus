using Microsoft.EntityFrameworkCore;
using SBInteligencia.Entities.Analytics;
using SBInteligencia.Entities.Informes;

namespace SBInteligencia.Entities;

public class SBInteligenciaDbContext : DbContext
{
    public SBInteligenciaDbContext(DbContextOptions<SBInteligenciaDbContext> options)
        : base(options)
    {
    }

    // 🔹 INFORMES
    public DbSet<InformesHecho> InformesHechos { get; set; }
    public DbSet<InformesHechosDetalle> InformesHechosDetalles { get; set; }

    // 🔹 ANALYTICS
    public DbSet<CoberturaResumen> CoberturaResumen { get; set; }
    public DbSet<CoberturaMensual> CoberturaMensual { get; set; }
    public DbSet<Partido> Partidos { get; set; }
    public DbSet<AreaResponsabilidad> AreaResponsabilidad { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("utf8mb4_0900_ai_ci")
                    .HasCharSet("utf8mb4");

        // 🔹 INFORMES
        modelBuilder.Entity<InformesHecho>(entity =>
        {
            entity.HasKey(e => e.IdInforme);
            entity.Property(e => e.Activo).HasDefaultValueSql("'1'");
            entity.Property(e => e.FechaCreacion).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<InformesHechosDetalle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(d => d.IdInformeNavigation)
                .WithMany(p => p.InformesHechosDetalles)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        // 🔹 COBERTURA
        modelBuilder.Entity<CoberturaResumen>(entity =>
        {
            entity.HasKey(e => e.Id);
        });

        modelBuilder.Entity<CoberturaMensual>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}