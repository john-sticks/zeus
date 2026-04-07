using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SBInteligencia.Controllers
{
    [Authorize]
    public class InformesController : Controller
    {

        public IActionResult Detalle(int? id)
        {
            if (id.HasValue)
            {
                ViewBag.IdInforme = id.Value;
                ViewBag.Modo = "guardado";
                return View();
            }

            var json = HttpContext.Session.GetString("InformeTemp");

            if (!string.IsNullOrEmpty(json))
            {
                // 🔥 IMPORTANTE: deserializar
                var obj = System.Text.Json.JsonSerializer.Deserialize<object>(json);

                ViewBag.Data = json;
                ViewBag.Modo = "preview";
                return View();
            }

            ViewBag.Modo = "vacio";
            return View();
        }
    }
}