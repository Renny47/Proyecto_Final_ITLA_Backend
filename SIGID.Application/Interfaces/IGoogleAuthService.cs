using SIGID.Application.DTOs;

namespace SIGID.Application.Interfaces;

public interface IGoogleAuthService
{
    Task<LoginResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request);
}