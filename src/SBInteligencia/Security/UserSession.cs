namespace SBInteligencia.Security
{
    public class UserSession
    {
        public string Nombre { get; set; }
        public string Token { get; set; }

        public string Rol { get; set; }
        public string Dependencia { get; set; }
    }
}