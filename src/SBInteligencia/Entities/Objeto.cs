using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SBInteligencia.Entities;

[Table("objetos")]
[Index("IdHecho", Name = "id_hecho")]
[Index("Implicacion", Name = "idx_objetos_implicacion")]
public partial class Objeto
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_hecho")]
    public int IdHecho { get; set; }

    [Column("tipo")]
    [StringLength(50)]
    public string Tipo { get; set; } = null!;

    [Column("marca")]
    [StringLength(50)]
    public string? Marca { get; set; }

    [Column("modelo")]
    [StringLength(50)]
    public string? Modelo { get; set; }

    [Column("cantidad")]
    [StringLength(50)]
    public string? Cantidad { get; set; }

    [Column("valor")]
    [StringLength(50)]
    public string? Valor { get; set; }

    [Column("descripcion")]
    [StringLength(1000)]
    public string? Descripcion { get; set; }

    [Column("implicacion")]
    [StringLength(50)]
    public string Implicacion { get; set; } = null!;

    [ForeignKey("IdHecho")]
    [InverseProperty("Objetos")]
    public virtual DatosHecho IdHechoNavigation { get; set; } = null!;
}
