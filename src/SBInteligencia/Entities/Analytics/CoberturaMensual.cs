using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SBInteligencia.Entities.Analytics;

[Table("cobertura_mensual")]
[Index("Anio", "Mes", Name = "idx_anio_mes")]
[Index("PartidoId", Name = "idx_partido")]
[Index("Anio", "Mes", "PartidoId", Name = "uk_anio_mes_partido", IsUnique = true)]
public partial class CoberturaMensual
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("anio")]
    public int Anio { get; set; }

    [Column("mes")]
    public int Mes { get; set; }

    [Column("partido_id")]
    public int PartidoId { get; set; }

    [Column("total_hechos")]
    public int? TotalHechos { get; set; }

    [Column("total_imputados")]
    public int? TotalImputados { get; set; }
}
