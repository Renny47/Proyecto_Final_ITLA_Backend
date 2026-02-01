using SIGID.Application.DTOs;

namespace SIGID.Application.Interfaces;

public interface IGoogleAuthService
{
    Task<LoginResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request);
    Task<LoginResponseDto> ExchangeCodeForTokenAsync(string authorizationCode);
    string GetGoogleLoginUrl();
}