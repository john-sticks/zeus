namespace SBInteligencia.Security
{
    public class MockAuthService : IAuthService
    {
        public Task<LoginResponse?> LoginAsync(string usuario, string password)
        {
            if (usuario == "admin" && password == "1234")
            {
                return Task.FromResult<LoginResponse?>(new LoginResponse
                {
                    Usuario = usuario,
                    Token = "ariel"
                });
            }

            return Task.FromResult<LoginResponse?>(null);
        }

        public Task<UserSession?> GetUserInfo(string token)
        {
            return Task.FromResult<UserSession?>(new UserSession
            {
                Nombre = "Administrador Mock",
                Token = token,

                // 🔥 CLAVE: agregar estos
                Rol = "ADMINISTRADOR",
                Dependencia = "SISTEMAS"
            });
        }
    }
}