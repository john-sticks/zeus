using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SBInteligencia.Entities.Analytics;

[Table("AreaResponsabilidad")]
public partial class AreaResponsabilidad
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [StringLength(50)]
    public string AmbitoResponsabilidad { get; set; } = null!;

    [InverseProperty("IdAreaResponsabilidadNavigation")]
    public virtual ICollection<Partido> Partidos { get; set; } = new List<Partido>();
}
