using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SBInteligencia.Entities.Analytics;

[Index("IdAreaResponsabilidad", Name = "idx_area")]
public partial class Partido
{
    [Key]
    [Column("idPartido")]
    public int IdPartido { get; set; }

    [StringLength(200)]
    public string Nombre { get; set; } = null!;

    [Column("idAreaResponsabilidad")]
    public int IdAreaResponsabilidad { get; set; }

    [ForeignKey("IdAreaResponsabilidad")]
    [InverseProperty("Partidos")]
    public virtual AreaResponsabilidad IdAreaResponsabilidadNavigation { get; set; } = null!;
}
