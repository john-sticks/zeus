namespace SBInteligencia.Security
{
    public class MenuItem
    {
        public string Text { get; set; }
        public string Icon { get; set; }
        public string Url { get; set; }

        // 🔥 para el siguiente paso (submenú)
        public List<MenuItem> Children { get; set; } = new List<MenuItem>();
    }
}