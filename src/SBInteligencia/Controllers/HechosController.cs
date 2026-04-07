using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBInteligencia.Services;

namespace SBInteligencia.Controllers
{
    [Authorize]
    public class HechosController : Controller
    {
        private readonly HechoService _service;

        public HechosController(HechoService service)
        {
            _service = service;
        }

        public IActionResult Consulta()
        {
            return View();
        }

        // 🔹 ESTE ES EL CORRECTO
        public async Task<IActionResult> Detalle(int id, int anio)
        {
            var hecho = await _service.GetHechoDetalle(id, anio);

            if (hecho == null)
                return NotFound();

            return View(hecho);
        }
        public IActionResult Involucrados()
        {
            return View();
        }
    }
}