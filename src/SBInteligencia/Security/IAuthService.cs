namespace SBInteligencia.Security
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(string usuario, string password);
        Task<UserSession?> GetUserInfo(string token);
    }
}