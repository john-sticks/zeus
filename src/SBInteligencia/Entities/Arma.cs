using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SBInteligencia.Entities;

[Table("armas")]
[Index("IdHecho", Name = "id_hecho")]
[Index("Marca", Name = "idx_armas_marca")]
public partial class Arma
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_hecho")]
    public int IdHecho { get; set; }

    [Column("tipo_arma")]
    [StringLength(100)]
    public string TipoArma { get; set; } = null!;

    [Column("marca")]
    [StringLength(50)]
    public string Marca { get; set; } = null!;

    [Column("modelo")]
    [StringLength(50)]
    public string? Modelo { get; set; }

    [Column("nro_serie")]
    [StringLength(50)]
    public string? NroSerie { get; set; }

    [Column("calibre")]
    [StringLength(50)]
    public string? Calibre { get; set; }

    [Column("observaciones")]
    [StringLength(1000)]
    public string? Observaciones { get; set; }

    [Column("implicacion")]
    [StringLength(50)]
    public string Implicacion { get; set; } = null!;

    [ForeignKey("IdHecho")]
    [InverseProperty("Armas")]
    public virtual DatosHecho IdHechoNavigation { get; set; } = null!;
}
