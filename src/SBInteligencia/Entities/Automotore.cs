using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SBInteligencia.Entities;

[Table("automotores")]
[Index("IdHecho", Name = "id_hecho")]
[Index("Dominio", Name = "idx_automotores_dominio")]
[Index("Marca", Name = "idx_automotores_marca")]
[Index("Modelo", Name = "idx_automotores_modelo")]
[Index("Vinculo", Name = "idx_automotores_vinculo")]
public partial class Automotore
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_hecho")]
    public int IdHecho { get; set; }

    [Column("marca")]
    [StringLength(50)]
    public string Marca { get; set; } = null!;

    [Column("modelo")]
    [StringLength(50)]
    public string? Modelo { get; set; }

    [Column("color")]
    [StringLength(50)]
    public string? Color { get; set; }

    [Column("dominio")]
    [StringLength(50)]
    public string? Dominio { get; set; }

    [Column("nro_motor")]
    [StringLength(50)]
    public string? NroMotor { get; set; }

    [Column("nro_chasis")]
    [StringLength(50)]
    public string? NroChasis { get; set; }

    [Column("vinculo")]
    [StringLength(50)]
    public string Vinculo { get; set; } = null!;

    [ForeignKey("IdHecho")]
    [InverseProperty("Automotores")]
    public virtual DatosHecho IdHechoNavigation { get; set; } = null!;
}
