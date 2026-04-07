using Microsoft.EntityFrameworkCore;
using SBInteligencia.DTO;
using SBInteligencia.Entities;

namespace SBInteligencia.Services
{
    public class DashboardService
    {
        private readonly SBInteligenciaDbContext _db;

        public DashboardService(SBInteligenciaDbContext db)
        {
            _db = db;
        }

        public async Task<DashboardDTO> GetDashboard()
        {
            var dto = new DashboardDTO();

            dto.TotalInformes = await _db.InformesHechos
                .CountAsync(x => x.Activo);

            var resumen = await _db.CoberturaResumen
                .GroupBy(x => 1)
                .Select(g => new
                {
                    TotalHechos = g.Sum(x => x.TotalHechos),
                    HechosDesde = g.Min(x => x.HechosDesde),
                    HechosHasta = g.Max(x => x.HechosHasta),

                    TotalImputados = g.Sum(x => x.TotalImputados),
                    ImputadosDesde = g.Min(x => x.ImputadosDesde),
                    ImputadosHasta = g.Max(x => x.ImputadosHasta)
                })
                .FirstOrDefaultAsync();

            if (resumen != null)
            {
                dto.TotalHechos = resumen.TotalHechos;
                dto.HechosDesde = resumen.HechosDesde?.ToDateTime(TimeOnly.MinValue);
                dto.HechosHasta = resumen.HechosHasta?.ToDateTime(TimeOnly.MinValue);

                dto.TotalInvolucrados = resumen.TotalImputados;
                dto.InvolucradosDesde = resumen.ImputadosDesde?.ToDateTime(TimeOnly.MinValue);
                dto.InvolucradosHasta = resumen.ImputadosHasta?.ToDateTime(TimeOnly.MinValue);
            }

            return dto;
        }
    }
}
