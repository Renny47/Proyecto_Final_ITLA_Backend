namespace SIGID.Application.DTOs;

public class LoginResponseDto
{
    public bool IsSuccess { get; set; }
    public bool Success => IsSuccess; // Alias para compatibilidad
    public string Message { get; set; } = string.Empty;
    public UserDto? User { get; set; }
    public string Token { get; set; } = string.Empty;
    public string AccessToken => Token; // Alias para compatibilidad
    public DateTime? ExpiresAt { get; set; }
}