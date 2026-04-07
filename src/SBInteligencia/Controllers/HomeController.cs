using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBInteligencia.Models;
using SBInteligencia.Services;
using System.Diagnostics;

namespace SBInteligencia.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DashboardService _dashboardService;
        public HomeController(ILogger<HomeController> logger, DashboardService dashboardService)
        {
            _logger = logger;
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            var data = await _dashboardService.GetDashboard();

            return View(data);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
