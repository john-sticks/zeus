using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace SBInteligencia.Entities.Informes;

[Table("informes_hechos")]
[Index("AreaResponsabilidad", Name = "idx_area")]
[Index("IdUsuarioCreador", Name = "idx_usuario")]
public partial class InformesHecho
{
    [Key]
    [Column("id_informe")]
    public int IdInforme { get; set; }

    [Column("nombre")]
    [StringLength(200)]
    public string Nombre { get; set; } = null!;

    [Column("fecha_creacion", TypeName = "datetime")]
    public DateTime FechaCreacion { get; set; }

    [Column("id_usuario_creador")]
    public int IdUsuarioCreador { get; set; }

    [Column("usuario_creador")]
    [StringLength(100)]
    public string UsuarioCreador { get; set; } = null!;

    [Column("area_responsabilidad")]
    [StringLength(100)]
    public string AreaResponsabilidad { get; set; } = null!;

    [Required]
    [Column("activo")]
    public bool Activo { get; set; }

    [InverseProperty("IdInformeNavigation")]
    public virtual ICollection<InformesHechosDetalle> InformesHechosDetalles { get; set; } = new List<InformesHechosDetalle>();
}
