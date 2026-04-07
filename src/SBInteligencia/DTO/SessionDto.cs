namespace SBInteligencia.DTO
{
    public class SessionDto
    {
        public string User { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string Rol { get; set; }

        public string Destino { get; set; } // 👈 igual que JSON
    }
}