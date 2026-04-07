using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SBInteligencia.Entities;

[Table("involucrados")]
[Index("IdHecho", Name = "id_hecho")]
[Index("Apellido", Name = "idx_involucrados_apellido")]
[Index("Involucrado1", Name = "idx_involucrados_involucrado")]
[Index("Nombre", Name = "idx_involucrados_nombre")]
[Index("NroDni", Name = "idx_involucrados_nro_dni")]
[Index("PaisOrigen", Name = "idx_involucrados_pais_origen")]
[Index("PartidoDomicilio", Name = "idx_involucrados_partido_domicilio")]
[Index("Profesion", Name = "idx_involucrados_profesion")]
public partial class Involucrado
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_hecho")]
    public int IdHecho { get; set; }

    [Column("involucrado")]
    [StringLength(30)]
    public string Involucrado1 { get; set; } = null!;

    [Column("pais_origen")]
    [StringLength(50)]
    public string? PaisOrigen { get; set; }

    [Column("tipo_dni")]
    [StringLength(10)]
    public string? TipoDni { get; set; }

    [Column("nro_dni")]
    [StringLength(20)]
    public string? NroDni { get; set; }

    [Column("genero")]
    [StringLength(20)]
    public string? Genero { get; set; }

    [Column("apellido")]
    [StringLength(50)]
    public string? Apellido { get; set; }

    [Column("nombre")]
    [StringLength(50)]
    public string? Nombre { get; set; }

    [Column("provincia_nacimiento")]
    [StringLength(50)]
    public string? ProvinciaNacimiento { get; set; }

    [Column("ciudad_nacimiento")]
    [StringLength(50)]
    public string? CiudadNacimiento { get; set; }

    [Column("fecha_nacimiento")]
    public DateOnly? FechaNacimiento { get; set; }

    [Column("profesion")]
    [StringLength(50)]
    public string? Profesion { get; set; }

    [Column("observaciones")]
    [StringLength(1000)]
    public string? Observaciones { get; set; }

    [Column("provincia_domicilio")]
    [StringLength(50)]
    public string? ProvinciaDomicilio { get; set; }

    [Column("partido_domicilio")]
    [StringLength(50)]
    public string? PartidoDomicilio { get; set; }

    [Column("localidad_domicilio")]
    [StringLength(50)]
    public string? LocalidadDomicilio { get; set; }

    [Column("calle_domicilio")]
    [StringLength(50)]
    public string? CalleDomicilio { get; set; }

    [Column("nro_domicilio")]
    [StringLength(20)]
    public string? NroDomicilio { get; set; }

    [Column("entre")]
    [StringLength(50)]
    public string? Entre { get; set; }

    [Column("piso")]
    [StringLength(20)]
    public string? Piso { get; set; }

    [Column("departamento")]
    [StringLength(20)]
    public string? Departamento { get; set; }

    [Column("caracteristicas_fisicas")]
    [StringLength(500)]
    public string? CaracteristicasFisicas { get; set; }

    [ForeignKey("IdHecho")]
    [InverseProperty("Involucrados")]
    public virtual DatosHecho IdHechoNavigation { get; set; } = null!;
}
