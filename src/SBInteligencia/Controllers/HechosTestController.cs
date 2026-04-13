using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBInteligencia.DTO;
using SBInteligencia.Security;
using SBInteligencia.Services;
using System.Linq;
using System.Text.Json;

namespace SBInteligencia.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/hechos")]
    public class HechosApiController : ControllerBase
    {
        private readonly HechoService _service;

        public HechosApiController(HechoService service)
        {
            _service = service;
        }
        [HttpGet("test-db")]
        public async Task<IActionResult> TestDb()
        {
            var result = await _service.TestConexion(2026);
            return Ok(result);
        }

        [HttpGet("test-db-query")]
        public async Task<IActionResult> TestDbQuery()
        {
            var result = await _service.TestQuery(2026);
            return Ok(result);
        }
        [HttpPost("buscar")]
        public async Task<IActionResult> Buscar([FromBody] FiltrosHechos f)
        {
            var result = await _service.BuscarHechos(f);
            return Ok(result);
        }

        // 🔹 OPCIONAL: endpoint detalle JSON
        [HttpGet("{id}/{anio}")]
        public async Task<IActionResult> GetDetalle(int id, int anio)
        {
            var hecho = await _service.GetHechoDetalle(id, anio);

            if (hecho == null)
                return NotFound();

            return Ok(hecho);
        }
        [HttpPost("guardar-session")]
        public IActionResult GuardarSession([FromBody] CrearInformeDto dto)
        {
            var json = HttpContext.Session.GetString("InformeTemp");

            InformeSessionDto session;

            int nuevos = 0;

            if (string.IsNullOrEmpty(json))
            {
                session = new InformeSessionDto
                {
                    IdInforme = dto.IdInforme,
                    Nombre = dto.Nombre,
                    Hechos = dto.Hechos ?? new List<HechoRefDto>()
                };

                nuevos = session.Hechos.Count;
            }
            else
            {
                session = JsonSerializer.Deserialize<InformeSessionDto>(json);

                if (dto.Reemplazar)
                {
                    session.Hechos = dto.Hechos ?? new List<HechoRefDto>();
                }
                else
                {
                    nuevos = dto.Hechos
                        .Count(h => !session.Hechos.Any(s => s.IdHecho == h.IdHecho));

                    session.Hechos = session.Hechos
                        .Concat(dto.Hechos)
                        .GroupBy(x => x.IdHecho)
                        .Select(g => g.First())
                        .ToList();
                }

                if (dto.IdInforme.HasValue)
                    session.IdInforme = dto.IdInforme;

                if (!string.IsNullOrWhiteSpace(dto.Nombre))
                    session.Nombre = dto.Nombre;
            }

            HttpContext.Session.SetString("InformeTemp",
                JsonSerializer.Serialize(session));

            return Ok(new
            {
                total = session.Hechos.Count,
                nuevos = nuevos
            });
        }
    }

    [ApiController]
    [Route("api/involucrados")]
    public class InvolucradosApiController : ControllerBase
    {
        private readonly HechoService _service;

        public InvolucradosApiController(HechoService service)
        {
            _service = service;
        }

        [HttpPost("buscar")]
        public async Task<IActionResult> Buscar([FromBody] FiltrosInvolucrados f)
        {
            var result = await _service.BuscarInvolucrados(f);
            return Ok(result);
        }

        [HttpGet("{id}/{anio}")]
        public async Task<IActionResult> GetDetalle(int id, int anio)
        {
            var inv = await _service.GetInvolucradoDetalle(id, anio);

            if (inv == null)
                return NotFound();

            return Ok(inv);
        }
    }
    [ApiController]
    [Route("api/cobertura")]
    public class CoberturaApiController : ControllerBase
    {
        private readonly CoberturaService _service;

        public CoberturaApiController(CoberturaService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var data = await _service.GetResumen();
            return Ok(data);
        }
        [HttpGet("calendario")]
        public async Task<IActionResult> GetCalendario(int partidoId)
        {
            var data = await _service.ObtenerCalendario(partidoId);
            return Ok(data);
        }
        [HttpGet("resumen-global")]
        public async Task<IActionResult> GetResumenGlobal()
        {
            var data = await _service.ObtenerCoberturaGlobal();
            return Ok(data);
        }
    }
    [ApiController]
    [Route("api/informe")]
    public class InformeApiController : ControllerBase
    {
        private readonly InformeService _service;

        public InformeApiController(InformeService service)
        {
            _service = service;
        }

        private UserSession GetUser()
        {
            return new UserSession
            {
                Nombre = User.Identity?.Name ?? "",
                Dependencia = User.FindFirst("Dependencia")?.Value
            };
        }
        [HttpPost("preview")]
        public async Task<IActionResult> Preview([FromBody] CrearInformeDto dto)
        {
            try
            {
                if (dto == null)
                    return BadRequest("DTO NULL");

                var data = await _service.GetPreview(dto);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // 🔥 CLAVE
            }
        }

        [HttpPost("crear")]
        public async Task<IActionResult> Crear([FromBody] CrearInformeDto dto)
        {
            var user = GetUser();

            dto.Usuario = user.Nombre;
            dto.Area = user.Dependencia;

            var id = await _service.CrearInforme(dto);

            return Ok(new { id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var data = await _service.GetInforme(id);

            if (data == null)
                return NotFound();

            return Ok(data);
        }
        [HttpGet]
        public IActionResult Ping()
        {
            return Ok("PING INFORME NUEVO");
        }
        [HttpGet("listado")]
        public async Task<IActionResult> GetListado()
        {
            var data = await _service.GetInformes();
            return Ok(data);
        }
        [HttpPost("cerrar-session")]
        public IActionResult CerrarSession()
        {
            HttpContext.Session.Remove("InformeTemp");
            return Ok();
        }
        [HttpGet("session")]
        public IActionResult GetSession()
        {
            var json = HttpContext.Session.GetString("InformeTemp");

            if (string.IsNullOrEmpty(json))
                return NoContent();

            var obj = JsonSerializer.Deserialize<InformeSessionDto>(json);

            return Ok(obj);
        }

        [HttpPost("eliminar-informe")]
        public async Task<IActionResult> EliminarInforme([FromBody] int idInforme)
        {
            var ok = await _service.EliminarInforme(idInforme);

            if (!ok)
                return NotFound();

            return Ok();
        }
        [HttpPost("actualizar")]
        public async Task<IActionResult> Actualizar([FromBody] CrearInformeDto dto)
        {
            var user = GetUser();

            dto.Usuario = user.Nombre;
            dto.Area = user.Dependencia;

            var ok = await _service.ActualizarInforme(dto);

            return Ok(ok);
        }
        [HttpPost("eliminar")]
        public async Task<IActionResult> EliminarHecho([FromBody] EliminarHechoDto dto)
        {
            var ok = await _service.EliminarHecho(dto.IdInforme, dto.IdHecho);

            if (!ok)
                return NotFound();

            return Ok();
        }
        [HttpPost("cargar-en-session")]
        public async Task<IActionResult> CargarEnSession([FromBody] int id)
        {
            HttpContext.Session.Remove("InformeTemp");
            var dto = await _service.GetInformeIds(id);

            HttpContext.Session.SetString("InformeTemp",
                JsonSerializer.Serialize(dto));

            return Ok();
        }

    }
}