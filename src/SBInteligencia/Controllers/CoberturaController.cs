using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SBInteligencia.Controllers
{
    public class CoberturaController : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }
    }
}
