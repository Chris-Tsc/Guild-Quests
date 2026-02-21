namespace BLL.Services.Interfaces
{
    public interface IAuthenticationService
    {
        Task<string> RegisterAsync(string username, string password);
        Task<string> LoginAsync(string username, string password);
    }
}