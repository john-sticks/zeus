using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace SBInteligencia.Entities;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Arma> Armas { get; set; }

    public virtual DbSet<Automotore> Automotores { get; set; }

    public virtual DbSet<DatosHecho> DatosHecho { get; set; }

    public virtual DbSet<Involucrado> Involucrado { get; set; }

    public virtual DbSet<Objeto> Objetos { get; set; }

    public virtual DbSet<Secuestro> Secuestros { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=10.200.0.79;port=3306;database=delitos_2026;user=ariel;password=Tetratetra45+", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.45-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Arma>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.IdHechoNavigation).WithMany(p => p.Armas).HasConstraintName("armas_ibfk_1");
        });

        modelBuilder.Entity<Automotore>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.IdHechoNavigation).WithMany(p => p.Automotores).HasConstraintName("automotores_ibfk_1");
        });

        modelBuilder.Entity<DatosHecho>(entity =>
        {
            entity.HasKey(e => e.IdHecho).HasName("PRIMARY");

            entity.HasIndex(e => e.Relato, "idx_datos_hecho_relato").HasAnnotation("MySql:FullTextIndex", true);

            entity.Property(e => e.IdHecho).ValueGeneratedNever();
        });

        modelBuilder.Entity<Involucrado>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.IdHechoNavigation).WithMany(p => p.Involucrados).HasConstraintName("involucrados_ibfk_1");
        });

        modelBuilder.Entity<Objeto>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.IdHechoNavigation).WithMany(p => p.Objetos).HasConstraintName("objetos_ibfk_1");
        });

        modelBuilder.Entity<Secuestro>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.IdHechoNavigation).WithMany(p => p.Secuestros).HasConstraintName("secuestros_ibfk_1");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
