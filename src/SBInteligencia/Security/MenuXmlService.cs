using System.Xml.Linq;
using System.Security.Claims;

namespace SBInteligencia.Security
{
    public class MenuXmlService
    {
        private readonly IWebHostEnvironment _env;

        // 🔥 CACHE EN MEMORIA POR NIVEL
        private static readonly Dictionary<string, XDocument> _cache = new();

        public MenuXmlService(IWebHostEnvironment env)
        {
            _env = env;
        }

        // 🔹 MENU VISIBLE
        public List<MenuItem> GetMenu(ClaimsPrincipal user)
        {
            var doc = GetXml(user);

            var navigation = doc.Root?.Element("navigation");

            if (navigation == null)
                return new List<MenuItem>();

            return navigation.Elements("page")
                .Select(x => new MenuItem
                {
                    Text = x.Attribute("title")?.Value,
                    Icon = x.Attribute("icon")?.Value,
                    Url = MapUrl(x.Attribute("href")?.Value)
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Url))
                .ToList();
        }

        // 🔹 URLs permitidas (middleware)
        public List<string> GetAllowedUrls(ClaimsPrincipal user)
        {
            var doc = GetXml(user);

            return doc.Descendants("page")
                .Select(x => MapUrl(x.Attribute("href")?.Value))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
        }

        // 🔹 PERMISOS (APIs)
        public List<string> GetPermissions(ClaimsPrincipal user)
        {
            var doc = GetXml(user);

            return doc.Descendants("permission")
                .Select(x => x.Attribute("name")?.Value?.ToUpper())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct()
                .ToList();
        }

        // 🔥 CENTRALIZA TODO
        private XDocument GetXml(ClaimsPrincipal user)
        {
            var rol = user.FindFirst(ClaimTypes.Role)?.Value;
            var nivel = MapRolToNivel(rol);

            if (_cache.ContainsKey(nivel))
                return _cache[nivel];

            var file = $"permisos_{nivel}.xml";
            var path = Path.Combine(_env.ContentRootPath, "Config", "Permisos", file);

            if (!File.Exists(path))
                return new XDocument();

            var doc = XDocument.Load(path);

            _cache[nivel] = doc;

            return doc;
        }

        // 🔹 MAPEO DE ROLES
        private string MapRolToNivel(string rol)
        {
            if (string.IsNullOrWhiteSpace(rol))
                return "30";

            rol = rol.ToUpper().Trim();

            return rol switch
            {
                "ADMINISTRADOR" => "10",
                "SUPERVISOR" => "15",
                "ANALISTA" => "20",
                "OPERADOR" => "20",
                "CONSULTOR" => "30",
                "ESTRATEGICO" => "30",
                _ => "30"
            };
        }

        // 🔹 NORMALIZACIÓN URL
        private string MapUrl(string href)
        {
            if (string.IsNullOrWhiteSpace(href))
                return "";

            return href
                .Replace("~/", "/")
                .Replace(".aspx", "")
                .ToLower()
                .TrimEnd('/');
        }
    }
}