using SIGID.Application.DTOs;

namespace SIGID.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest);
    Task<LoginResponseDto> RegisterAsync(RegisterRequestDto registerRequest);
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<bool> ValidateTokenAsync(string token);
}