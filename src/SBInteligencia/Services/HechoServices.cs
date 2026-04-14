using Microsoft.EntityFrameworkCore;
using SBInteligencia.DTO;
using SBInteligencia.Entities;
using SBInteligencia.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace SBInteligencia.Services
{
    public class HechoService
    {
        private readonly IAppDbContextFactory _factory;

        public HechoService(IAppDbContextFactory factory)
        {
            _factory = factory;
        }

        public async Task<PagedResult<HechoListadoDto>> BuscarHechos(FiltrosHechos f)
        {

            if (f.FechaDesde.Year != f.FechaHasta.Year)
                throw new Exception("El rango de fechas debe estar dentro del mismo año.");

            if (f.Anio != f.FechaDesde.Year)
                throw new Exception("El año no coincide con el rango de fechas.");

            using var db = _factory.Create(f.Anio);

            var desde = f.FechaDesde.ToString("yyyy-MM-dd");
            var hasta = f.FechaHasta.ToString("yyyy-MM-dd");

            var where = new List<string>();

            // 🔹 FECHA (SIEMPRE)
            where.Add($"fecha_carga BETWEEN '{desde}' AND '{hasta}'");

            // 🔹 IPP
            if (f.UsarIPP && !string.IsNullOrWhiteSpace(f.IPP))
                where.Add($"ipp LIKE '%{f.IPP}%'");

            // 🔹 PARTIDO (MULTI)
            if (f.Partido?.Any() == true)
            {
                var partidos = string.Join(",", f.Partido.Select(p => $"'{p}'"));
                where.Add($"partido_hecho IN ({partidos})");
            }

            // 🔹 LOCALIDAD
            if (f.UsarLocalidad && !string.IsNullOrWhiteSpace(f.Localidad))
                where.Add($"localidad_hecho = '{f.Localidad}'");

            // 🔹 CALIFICACIONES (MULTI LIKE)
            if (f.Calificaciones?.Any() == true)
            {
                var califs = f.Calificaciones
                    .Select(c => $"calificaciones LIKE '%{c}%'");

                where.Add("(" + string.Join(" OR ", califs) + ")");
            }

            // 🔹 RELATO (FULLTEXT)
            if (f.UsarRelato && !string.IsNullOrWhiteSpace(f.Relato))
            {
                var fullText = BuildFullTextQuery(f.Relato);
                where.Add($"MATCH(relato) AGAINST ('{fullText}' IN BOOLEAN MODE)");
            }

            // 🔹 DOMICILIO
            if (f.UsarDomicilio && !string.IsNullOrWhiteSpace(f.Calle))
                where.Add($"calle LIKE '%{f.Calle}%'");

            // 🔹 GEO
            if (f.UsarGeo && f.Latitud.HasValue && f.Longitud.HasValue)
            {
                var radio = f.RadioMetros <= 0 ? 1000 : f.RadioMetros;

                var lat = f.Latitud.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);
                var lng = f.Longitud.Value.ToString(System.Globalization.CultureInfo.InvariantCulture);

                where.Add($@"
                    latitud IS NOT NULL
                    AND longitud IS NOT NULL
                    AND latitud <> ''
                    AND longitud <> ''
    
                    -- 🔥 VALIDACIÓN REAL
                    AND CAST(latitud AS DECIMAL(10,6)) BETWEEN -90 AND 90
                    AND CAST(longitud AS DECIMAL(10,6)) BETWEEN -180 AND 180

                    AND ST_Distance_Sphere(
                        POINT(
                            CAST(longitud AS DECIMAL(10,6)),
                            CAST(latitud AS DECIMAL(10,6))
                        ),
                        POINT({lng}, {lat})
                    ) <= {radio}
                ");
            }

            int pagina = f.Pagina <= 0 ? 1 : f.Pagina;
            int tamaño = f.TamañoPagina <= 0 ? 100 : f.TamañoPagina;
            if (tamaño > 1000) tamaño = 1000;

            int skip = (pagina - 1) * tamaño;

            var sql = $@"
        SELECT 
            id_hecho,
            nro_registro,
            ipp,
            fecha_carga,
            partido_hecho,
            localidad_hecho,
            calle,
            altura,
            latitud,
            longitud,
            calificaciones,
            dependencia
            {(f.UsarRelato ? ", relato" : "")}
        FROM datos_hecho
        WHERE {string.Join(" AND ", where)}
        ORDER BY fecha_carga DESC
        LIMIT {tamaño} OFFSET {skip}
    ";

            Console.WriteLine(sql);

            // 🔥 EJECUTAR QUERY BASE
            var baseQuery = db.DatosHecho
                .FromSqlRaw(sql)
                .AsNoTracking();



            try
            {
                // 🟢 CASO LIVIANO (sin relato)
                var data = await baseQuery
                    .Select(x => new HechoListadoDto
                    {
                        IdHecho = x.IdHecho,
                        NroRegistro = x.NroRegistro,
                        Ipp = x.Ipp,
                        FechaCarga = x.FechaCarga,
                        Partido = x.PartidoHecho,
                        Localidad = x.LocalidadHecho,
                        Calle = x.Calle,
                        Altura = x.Altura,
                        Latitud = x.Latitud,
                        Longitud = x.Longitud,
                        Calificaciones = x.Calificaciones,
                        Dependencia = x.Dependencia
                        // ❌ NO relato
                    })
                    .ToListAsync();

                return new PagedResult<HechoListadoDto>
                {
                    Pagina = pagina,
                    TamañoPagina = tamaño,
                    Data = data
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR REAL:");
                Console.WriteLine(ex.ToString());
                throw;
            }

        }
        private string BuildFullTextQuery(string input)
        {
            var palabras = input
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => "+" + p); // AND lógico

            return string.Join(" ", palabras);
        }
        public async Task<HechoDetalleDto> GetHechoDetalle(int id, int anio)
        {
            using var db = _factory.Create(anio);
            var conn = db.Database.GetDbConnection();

            await conn.OpenAsync();

            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
        SELECT 
            id_hecho,
            nro_registro,
            ipp,
            fecha_carga,
            partido_hecho,
            localidad_hecho,
            calle,
            altura,
            latitud,
            longitud,
            calificaciones,
            dependencia,
            relato
        FROM datos_hecho
        WHERE id_hecho = @id
        LIMIT 1
    ";

            var param = cmd.CreateParameter();
            param.ParameterName = "@id";
            param.Value = id;
            cmd.Parameters.Add(param);

            using var reader = await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new HechoDetalleDto
            {
                IdHecho = reader.GetInt32("id_hecho"),
                NroRegistro = reader["nro_registro"]?.ToString(),
                Ipp = reader["ipp"]?.ToString(),
                FechaCarga = reader.GetFieldValue<DateOnly>(reader.GetOrdinal("fecha_carga")),
                Partido = reader["partido_hecho"]?.ToString(),
                Localidad = reader["localidad_hecho"]?.ToString(),
                Calle = reader["calle"]?.ToString(),
                Altura = reader["altura"]?.ToString(),

                LatitudRaw = reader["latitud"]?.ToString(),
                LongitudRaw = reader["longitud"]?.ToString(),

                Latitud = ParseLat(reader["latitud"]?.ToString()),
                Longitud = ParseLng(reader["longitud"]?.ToString()),

                Calificaciones = reader["calificaciones"]?.ToString(),
                Dependencia = reader["dependencia"]?.ToString(),
                Relato = reader["relato"]?.ToString()
            };
        }
        private double? ParseLat(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            value = value.Replace(",", ".");

            if (double.TryParse(value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var result))
            {
                if (result >= -90 && result <= 90)
                    return result;
            }

            return null;
        }

        private double? ParseLng(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            value = value.Replace(",", ".");

            if (double.TryParse(value, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var result))
            {
                if (result >= -180 && result <= 180)
                    return result;
            }

            return null;
        }
        public async Task<PagedResult<InvolucradoListadoDto>> BuscarInvolucrados(FiltrosInvolucrados f)
        {
            using var db = _factory.Create(f.Anio);

            var where = new List<string>();

            if (!string.IsNullOrWhiteSpace(f.Nombre))
                where.Add($"nombre LIKE '%{f.Nombre}%'");

            if (!string.IsNullOrWhiteSpace(f.Apellido))
                where.Add($"apellido LIKE '%{f.Apellido}%'");

            if (!string.IsNullOrWhiteSpace(f.NroDni))
                where.Add($"nro_dni LIKE '%{f.NroDni}%'");

            int pagina = f.Pagina <= 0 ? 1 : f.Pagina;
            int tamaño = f.TamañoPagina <= 0 ? 100 : f.TamañoPagina;

            int skip = (pagina - 1) * tamaño;

            var sql = $@"
        SELECT 
            id,
            id_hecho,
            involucrado,
            tipo_dni,
            nro_dni,
            apellido,
            nombre
        FROM involucrados
        {(where.Any() ? "WHERE " + string.Join(" AND ", where) : "")}
        ORDER BY id DESC
        LIMIT {tamaño} OFFSET {skip}
    ";

            var data = await db.Involucrado
                .FromSqlRaw(sql)
                .AsNoTracking()
                .Select(x => new InvolucradoListadoDto
                {
                    Id = x.Id,
                    IdHecho = x.IdHecho,
                    TipoDni = x.TipoDni,
                    NroDni = x.NroDni,
                    Apellido = x.Apellido,
                    Nombre = x.Nombre
                })
                .ToListAsync();

            return new PagedResult<InvolucradoListadoDto>
            {
                Pagina = pagina,
                TamañoPagina = tamaño,
                Data = data
            };
        }
        public async Task<Involucrado> GetInvolucradoDetalle(int id,int anio)
        {
            using var db = _factory.Create(anio);
            return await db.Involucrado
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }
        public async Task<string> TestConexion(int anio)
        {
            try
            {
                using var db = _factory.Create(anio);

                // 🔥 SOLO abrir conexión (sin query pesada)
                await db.Database.OpenConnectionAsync();

                return "OPEN OK";
            }
            catch (Exception ex)
            {
                return $"ERROR OPEN: {ex.Message}";
            }
        }
        public async Task<string> TestQuery(int anio)
        {
            var connectionString =
                    "Server=10.200.0.79;Port=3306;User=ariel;Password=Tetratetra45+;Connection Timeout=5;";
            try
            {
                

                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();

                optionsBuilder.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(8, 0, 36))
                );

                // 🔥 LOG para ver si entra
                optionsBuilder
                    .LogTo(Console.WriteLine)
                    .EnableSensitiveDataLogging();

                using var db = new AppDbContext(optionsBuilder.Options);

                // 🔥 prueba simple
                var result = await db.Database.ExecuteSqlRawAsync("SELECT 1");

                return "QUERY OK";
            }
            catch (Exception ex)
            {
                return $"ERROR QUERY: {ex.Message + connectionString}" ;
            }
        }
    }
}