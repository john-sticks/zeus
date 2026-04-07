using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SBInteligencia.Entities.Analytics;

[Table("cobertura_resumen")]
[Index("Anio", Name = "idx_anio")]
[Index("PartidoId", Name = "idx_partido")]
[Index("Anio", "PartidoId", Name = "uk_anio_partido", IsUnique = true)]
public partial class CoberturaResumen
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("anio")]
    public int Anio { get; set; }

    [Column("partido_id")]
    public int PartidoId { get; set; }

    [Column("hechos_desde")]
    public DateOnly? HechosDesde { get; set; }

    [Column("hechos_hasta")]
    public DateOnly? HechosHasta { get; set; }

    [Column("total_hechos")]
    public int TotalHechos { get; set; }

    [Column("imputados_desde")]
    public DateOnly? ImputadosDesde { get; set; }

    [Column("imputados_hasta")]
    public DateOnly? ImputadosHasta { get; set; }

    [Column("total_imputados")]
    public int TotalImputados { get; set; }

    [Column("fecha_actualizacion", TypeName = "datetime")]
    public DateTime? FechaActualizacion { get; set; }
}
