using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SBInteligencia.DTO;
using SBInteligencia.Entities.Informes;
using SBInteligencia.Infrastructure.Data;

namespace SBInteligencia.Services
{
    public class InformeService
    {
        //private readonly InformesDbContext _informesDb;
        private readonly IAppDbContextFactory _factory;

        public InformeService(
            IAppDbContextFactory factory)
        {
            _factory = factory;
        }

        // =========================
        // CREAR INFORME
        // =========================
        public async Task<int> CrearInforme(CrearInformeDto dto)
        {
            using var db = _factory.CreateAnalytics();
            var informe = new InformesHecho
            {
                Nombre = dto.Nombre,
                FechaCreacion = DateTime.Now,
                UsuarioCreador = dto.Usuario,
                AreaResponsabilidad = dto.Area,
                Activo = true
            };

            db.InformesHechos.Add(informe);
            await db.SaveChangesAsync();

            var detalles = dto.Hechos.Select(h => new InformesHechosDetalle
            {
                IdInforme = informe.IdInforme,
                IdHecho = h.IdHecho,
                Anio = h.Anio
            });

            db.InformesHechosDetalles.AddRange(detalles);
            await db.SaveChangesAsync();

            return informe.IdInforme;
        }

        // =========================
        // OBTENER INFORME (MAPA)
        // =========================
        public async Task<InformeDetalleDto> GetInforme(int id)
        {
            using var db = _factory.CreateAnalytics();

            var informe = await db.InformesHechos
                .FirstOrDefaultAsync(x => x.IdInforme == id);

            if (informe == null)
                return null;

            var detalles = await db.InformesHechosDetalles
                .Where(x => x.IdInforme == id)
                .ToListAsync();

            var resultado = new List<PuntoMapaDto>();

            var grupos = detalles.GroupBy(x => x.Anio);

            foreach (var grupo in grupos)
            {
                using var db2 = _factory.Create(grupo.Key);

                var ids = grupo.Select(x => x.IdHecho).ToList();

                var hechosRaw = await db2.DatosHecho
                    .Where(x => ids.Contains(x.IdHecho))
                    .Select(x => new
                    {
                        x.IdHecho,
                        x.Latitud,
                        x.Longitud,
                        x.Ipp,
                        x.Calificaciones
                    })
                    .ToListAsync();

                var hechos = hechosRaw.Select(x => new PuntoMapaDto
                {
                    Id = x.IdHecho,
                    Latitud = ParseDouble(x.Latitud),
                    Longitud = ParseDouble(x.Longitud),
                    Titulo = x.Ipp,
                    Descripcion = x.Calificaciones,
                    Calificacion = x.Calificaciones
                }).ToList();

                resultado.AddRange(hechos);
            }

            return new InformeDetalleDto
            {
                IdInforme = id,
                Nombre = informe.Nombre, // 🔥 ACÁ ESTABA EL PROBLEMA
                Hechos = resultado
            };
        }
        private double? ParseDouble(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            value = value.Replace(",", ".");

            if (double.TryParse(value,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out var result))
            {
                return result;
            }

            return null;
        }
        public async Task<List<PuntoMapaDto>> GetPreview(CrearInformeDto dto)
        {
            if (dto.Hechos == null || !dto.Hechos.Any())
                return new List<PuntoMapaDto>();

            var resultado = new List<PuntoMapaDto>();

            var grupos = dto.Hechos.GroupBy(x => x.Anio);

            foreach (var grupo in grupos)
            {
                using var db = _factory.Create(grupo.Key);

                var ids = grupo.Select(x => x.IdHecho).ToList();

                var raw = await db.DatosHecho
                    .Where(x => ids.Contains(x.IdHecho))
                    .Select(x => new
                    {
                        x.IdHecho,
                        x.Latitud,
                        x.Longitud,
                        x.Ipp,
                        x.Calificaciones
                    })
                    .ToListAsync();

                resultado.AddRange(raw.Select(x => new PuntoMapaDto
                {
                    Id = x.IdHecho,
                    Latitud = ParseDouble(x.Latitud),
                    Longitud = ParseDouble(x.Longitud),
                    Titulo = x.Ipp,
                    Descripcion = x.Calificaciones,
                    Calificacion = x.Calificaciones
                }));
            }

            return resultado;
        }
        public async Task<List<InformesHecho>> GetInformes()
        {
            using var db = _factory.CreateAnalytics();
            return await db.InformesHechos
                .Where(x => x.Activo)
                .OrderByDescending(x => x.FechaCreacion)
                .ToListAsync();
        }
        // =========================
        // CREAR INFORME
        // =========================
        public async Task<bool> EliminarInforme(int idInforme)
        {
            using var db = _factory.CreateAnalytics();

            var informe = await db.InformesHechos
                .Where(x => x.IdInforme == idInforme && x.Activo)
                .FirstOrDefaultAsync();

            if (informe == null)
                return false;

            informe.Activo = false;

            await db.SaveChangesAsync();

            return true;
        }
        public async Task<bool> ActualizarInforme(CrearInformeDto dto)
        {
            using var db = _factory.CreateAnalytics();

            var informe = await db.InformesHechos
                .FirstOrDefaultAsync(x => x.IdInforme == dto.IdInforme);

            if (informe == null)
                return false;

            // 🔥 borrar detalles actuales
            var detalles = db.InformesHechosDetalles
                .Where(x => x.IdInforme == dto.IdInforme);

            db.InformesHechosDetalles.RemoveRange(detalles);

            if (dto.IdInforme == null)
                throw new Exception("IdInforme requerido");

            var nuevos = dto.Hechos.Select(h => new InformesHechosDetalle
            {
                IdInforme = dto.IdInforme.Value,
                IdHecho = h.IdHecho,
                Anio = h.Anio
            });

            db.InformesHechosDetalles.AddRange(nuevos);

            await db.SaveChangesAsync();

            return true;
        }
        public async Task<bool> EliminarHecho(int idInforme, int idHecho)
        {
            using var db = _factory.CreateAnalytics();

            var item = await db.InformesHechosDetalles
                .FirstOrDefaultAsync(x =>
                    x.IdInforme == idInforme &&
                    x.IdHecho == idHecho);

            if (item == null)
                return false;

            db.InformesHechosDetalles.Remove(item);

            await db.SaveChangesAsync();

            return true;
        }
        public async Task<InformeSessionDto> GetInformeIds(int id)
        {
            using var db = _factory.CreateAnalytics();

            var informe = await db.InformesHechos
                .FirstOrDefaultAsync(x => x.IdInforme == id);

            if (informe == null)
                return null;

            var detalles = await db.InformesHechosDetalles
                .Where(x => x.IdInforme == id)
                .ToListAsync();

            return new InformeSessionDto
            {
                IdInforme = id,
                Nombre = informe.Nombre,
                Hechos = detalles.Select(x => new HechoRefDto
                {
                    IdHecho = x.IdHecho,
                    Anio = x.Anio
                }).ToList()
            };
        }
    }

}