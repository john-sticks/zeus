using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SBInteligencia.Models;
using SBInteligencia.Security;
using System.Security.Claims;

namespace SBInteligencia.Controllers
{
    [AllowAnonymous]
    public class LoginController : Controller
    {
        private readonly IAuthService _auth;

        public LoginController(IAuthService auth)
        {
            _auth = auth;
        }

        [HttpGet]
        public IActionResult Index(string msg = null)
        {
            if (msg == "conflict")
                ViewBag.Error = "⚠ Tu sesión fue cerrada por otro login";

            if (msg == "expired")
                ViewBag.Error = "⏱ Sesión expirada";

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string Usuario, string Password)
        {
            if (string.IsNullOrWhiteSpace(Usuario) || string.IsNullOrWhiteSpace(Password))
            {
                ViewBag.Error = "Completar usuario y contraseña";
                return View();
            }

            // 🔥 USAR TU FLUJO ACTUAL
            var loginResult = await _auth.LoginAsync(Usuario, Password);

            if (loginResult == null || string.IsNullOrEmpty(loginResult.Token))
            {
                ViewBag.Error = "Usuario o contraseña incorrectos";
                return View();
            }

            var user = await _auth.GetUserInfo(loginResult.Token);

            // 🔥 CREAR CLAIMS (igual que ya hacés)
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Nombre),
            new Claim(ClaimTypes.Role, user.Rol.ToUpper().Trim()),
            new Claim("Dependencia", user.Dependencia),
            new Claim("Token", loginResult.Token)
        };

            var identity = new ClaimsIdentity(claims, "Cookies");
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync("Cookies", principal);

            return RedirectToAction("Index", "Home");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            return RedirectToAction("Index");
        }
    }
}