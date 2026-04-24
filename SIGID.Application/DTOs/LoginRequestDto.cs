namespace SIGID.Application.DTOs;

public class LoginRequestDto
{
    /// <summary>
    /// Username o Email del usuario (acepta ambos)
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Contraseña del usuario
    /// </summary>
    public string Password { get; set; } = string.Empty;
}