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
                //"ADMINISTRADOR" => "10",
                //"SUPERVISOR" => "15",
                //"ANALISTA" => "20",
                //"OPERADOR" => "20",
                //"CONSULTOR" => "30",
                //"ESTRATEGICO" => "30",
                //_ => "30"
                // 🔥 CLAVE: agregar estos
                Rol = "ADMINISTRADOR",
                Dependencia = "SISTEMAS"
            });
        }
    }
}