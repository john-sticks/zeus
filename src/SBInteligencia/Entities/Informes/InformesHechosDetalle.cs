using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SBInteligencia.Entities.Informes;

[Table("informes_hechos_detalle")]
[Index("Anio", Name = "idx_anio")]
[Index("IdHecho", Name = "idx_hecho")]
[Index("IdInforme", Name = "idx_informe")]
public partial class InformesHechosDetalle
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("id_informe")]
    public int IdInforme { get; set; }

    [Column("id_hecho")]
    public int IdHecho { get; set; }

    [Column("anio")]
    public int Anio { get; set; }

    [ForeignKey("IdInforme")]
    [InverseProperty("InformesHechosDetalles")]
    public virtual InformesHecho IdInformeNavigation { get; set; } = null!;
}
