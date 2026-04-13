using System.Xml.Linq;
using System.Security.Claims;

namespace SBInteligencia.Security
{
    public class MenuXmlService
    {
        private readonly IWebHostEnvironment _env;

        public MenuXmlService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public List<MenuItem> GetMenu(ClaimsPrincipal user)
        {
            var rol = user.FindFirst(ClaimTypes.Role)?.Value;

            // 🔥 MAPEO SIMPLE (después lo mejoramos)
            var file = $"permisos_{MapRolToNivel(rol)}.xml";

            var path = Path.Combine(_env.ContentRootPath, "Config", "Permisos", file);

            if (!File.Exists(path))
                return new List<MenuItem>();

            var doc = XDocument.Load(path);

            var navigation = doc.Root.Element("navigation");

            return navigation.Elements("page")
                .Select(x => new MenuItem
                {
                    Text = x.Attribute("title")?.Value,
                    Icon = x.Attribute("icon")?.Value,
                    Url = MapUrl(x.Attribute("href")?.Value)
                })
                .ToList();
        }

        private string MapRolToNivel(string rol)
        {
            return rol switch
            {
                "ADMIN" => "10",
                "OPERADOR" => "15",
                _ => "10"
            };
        }

        private string MapUrl(string href)
        {
            if (string.IsNullOrEmpty(href)) return "#";

            return href
                .Replace("~/", "/")
                .Replace(".aspx", "");
        }
    }
}