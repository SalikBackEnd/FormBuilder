using FormBuilder.Dtos;

namespace FormBuilder.Interfaces
{
    public interface IAuthService
    {
        Task<TokenResponse> RegisterAsync(RegisterRequest request);
        Task<TokenResponse> LoginAsync(LoginRequest request);
        Task<TokenResponse> RefreshTokenAsync(RefreshRequest request);
        Task LogoutAsync(Guid userId);
    }
}
