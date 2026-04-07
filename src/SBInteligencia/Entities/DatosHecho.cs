using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SBInteligencia.Entities;

[Table("datos_hecho")]
[Index("FechaCarga", Name = "idx_datos_hecho_fecha_carga")]
[Index("Ipp", Name = "idx_datos_hecho_ipp")]
[Index("LocalidadHecho", Name = "idx_datos_hecho_localidad_hecho")]
[Index("NroRegistro", Name = "idx_datos_hecho_nro_registro")]
[Index("PartidoHecho", Name = "idx_datos_hecho_partido_hecho")]
public partial class DatosHecho
{
    [Key]
    [Column("id_hecho")]
    public int IdHecho { get; set; }

    [Column("nro_registro")]
    [StringLength(30)]
    public string NroRegistro { get; set; } = null!;

    [Column("ipp")]
    [StringLength(30)]
    public string? Ipp { get; set; }

    [Column("fecha_carga")]
    public DateOnly FechaCarga { get; set; }

    [Column("hora_carga", TypeName = "time")]
    public TimeOnly HoraCarga { get; set; }

    [Column("dependencia")]
    [StringLength(100)]
    public string Dependencia { get; set; } = null!;

    [Column("fecha_inicio_hecho")]
    public DateOnly? FechaInicioHecho { get; set; }

    [Column("hora_inicio_hecho", TypeName = "time")]
    public TimeOnly? HoraInicioHecho { get; set; }

    [Column("partido_hecho")]
    [StringLength(50)]
    public string PartidoHecho { get; set; } = null!;

    [Column("localidad_hecho")]
    [StringLength(50)]
    public string? LocalidadHecho { get; set; }

    [Column("latitud")]
    [StringLength(50)]
    public string? Latitud { get; set; }

    [Column("calle")]
    [StringLength(50)]
    public string? Calle { get; set; }

    [Column("longitud")]
    [StringLength(50)]
    public string? Longitud { get; set; }

    [Column("altura")]
    [StringLength(10)]
    public string? Altura { get; set; }

    [Column("entre")]
    [StringLength(50)]
    public string? Entre { get; set; }

    [Column("calificaciones")]
    [StringLength(5000)]
    public string Calificaciones { get; set; } = null!;

    [Column("relato", TypeName = "mediumtext")]
    public string Relato { get; set; } = null!;

    [InverseProperty("IdHechoNavigation")]
    public virtual ICollection<Arma> Armas { get; set; } = new List<Arma>();

    [InverseProperty("IdHechoNavigation")]
    public virtual ICollection<Automotore> Automotores { get; set; } = new List<Automotore>();

    [InverseProperty("IdHechoNavigation")]
    public virtual ICollection<Involucrado> Involucrados { get; set; } = new List<Involucrado>();

    [InverseProperty("IdHechoNavigation")]
    public virtual ICollection<Objeto> Objetos { get; set; } = new List<Objeto>();

    [InverseProperty("IdHechoNavigation")]
    public virtual ICollection<Secuestro> Secuestros { get; set; } = new List<Secuestro>();
}
