using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using SBInteligencia.DTO;
using SBInteligencia.Infrastructure.Data;

namespace SBInteligencia.Services
{
    public class CoberturaService
    {
        private readonly IAppDbContextFactory _factory;

        public CoberturaService(IAppDbContextFactory factory)
        {
            _factory = factory;
        }

        public async Task<List<CoberturaDTO>> GetResumen()
        {
            using var db = _factory.CreateAnalytics();

            // 🔥 TRAER TODO A MEMORIA (solo lo necesario)
            var dataMensual = await db.CoberturaMensual
                .Select(x => new
                {
                    x.PartidoId,
                    x.Anio,
                    x.Mes,
                    TotalHechos = x.TotalHechos ?? 0,
                    TotalImputados = x.TotalImputados ?? 0
                })
                .ToListAsync();

            var agrupado = dataMensual
                .GroupBy(x => x.PartidoId)
                .Select(g =>
                {
                    var hechos = g.Where(x => x.TotalHechos > 0).ToList();
                    var imputados = g.Where(x => x.TotalImputados > 0).ToList();

                    return new
                    {
                        PartidoId = g.Key,

                        TotalHechos = g.Sum(x => x.TotalHechos),
                        TotalImputados = g.Sum(x => x.TotalImputados),

                        HechosDesde = hechos.Select(x => new DateTime(x.Anio, x.Mes, 1)).DefaultIfEmpty().Min(),
                        HechosHasta = hechos.Select(x => new DateTime(x.Anio, x.Mes, 1)).DefaultIfEmpty().Max(),

                        ImputadosDesde = imputados.Select(x => new DateTime(x.Anio, x.Mes, 1)).DefaultIfEmpty().Min(),
                        ImputadosHasta = imputados.Select(x => new DateTime(x.Anio, x.Mes, 1)).DefaultIfEmpty().Max()
                    };
                });

            var partidos = await db.Partidos.ToListAsync();

            return partidos
                .GroupJoin(
                    agrupado,
                    p => p.IdPartido,
                    c => c.PartidoId,
                    (p, c) => new { p, c = c.FirstOrDefault() }
                )
                .Select(x => new CoberturaDTO
                {
                    PartidoId = x.p.IdPartido,
                    Partido = x.p.Nombre,

                    TotalHechos = x.c?.TotalHechos ?? 0,
                    TotalImputados = x.c?.TotalImputados ?? 0,

                    HechosDesde = x.c?.HechosDesde != null
                    ? DateOnly.FromDateTime(x.c.HechosDesde)
                    : null,

                                HechosHasta = x.c?.HechosHasta != null
                    ? DateOnly.FromDateTime(x.c.HechosHasta)
                    : null,

                                ImputadosDesde = x.c?.ImputadosDesde != null
                    ? DateOnly.FromDateTime(x.c.ImputadosDesde)
                    : null,

                                ImputadosHasta = x.c?.ImputadosHasta != null
                    ? DateOnly.FromDateTime(x.c.ImputadosHasta)
                    : null
                })
                .OrderBy(x => x.Partido)
                .ToList();
        }

        public async Task<List<CoberturaCalendarioDto>> ObtenerCalendario(int partidoId)
        {
            using var db = _factory.CreateAnalytics();

            return await db.CoberturaMensual
                .Where(x => x.PartidoId == partidoId)
                .OrderBy(x => x.Anio)
                .ThenBy(x => x.Mes)
                .Select(x => new CoberturaCalendarioDto
                {
                    Anio = x.Anio,
                    Mes = x.Mes,
                    TotalHechos = x.TotalHechos ?? 0,
                    TotalImputados = x.TotalImputados ?? 0
                })
                .ToListAsync();
        }
        public async Task<List<CoberturaDTO>> ObtenerCoberturaGlobal()
        {
            using var db = _factory.CreateAnalytics();

            var agrupado = db.CoberturaMensual
                .GroupBy(x => x.PartidoId)
                .Select(g => new
                {
                    PartidoId = g.Key,
                    TotalHechos = g.Sum(x => x.TotalHechos ?? 0),
                    TotalImputados = g.Sum(x => x.TotalImputados ?? 0)
                });

            return await db.Partidos
                .GroupJoin(
                    agrupado,
                    p => p.IdPartido,
                    c => c.PartidoId,
                    (p, c) => new { p, c }
                )
                .SelectMany(
                    x => x.c.DefaultIfEmpty(),
                    (x, c) => new CoberturaDTO
                    {
                        PartidoId = x.p.IdPartido,
                        Partido = x.p.Nombre,

                        TotalHechos = c != null ? c.TotalHechos : 0,
                        TotalImputados = c != null ? c.TotalImputados : 0
                    }
                )
                .OrderBy(x => x.Partido)
                .ToListAsync();
        }
    }
}
