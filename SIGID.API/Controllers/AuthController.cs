using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;
using SIGID.Application.Security;

namespace SIGID.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IJwtGenerator jwtGenerator,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _jwtGenerator = jwtGenerator;
        _logger = logger;
    }

    /// <summary>
    /// Iniciar sesión de usuario
    /// </summary>
    /// <param name="request">Datos de login</param>
    /// <returns>Respuesta con token JWT</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "Datos inválidos", 
                    errors = ModelState 
                });
            }

            var result = await _authService.LoginAsync(request);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return Unauthorized(new { 
                success = false, 
                message = result.Message ?? "Credenciales inválidas" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el login para usuario: {UserName}", request.UserName);
            return StatusCode(500, new { 
                success = false, 
                message = "Error interno del servidor" 
            });
        }
    }

    /// <summary>
    /// Registrar nuevo usuario
    /// </summary>
    /// <param name="request">Datos de registro</param>
    /// <returns>Respuesta con token JWT</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { 
                    success = false, 
                    message = "Datos inválidos", 
                    errors = ModelState 
                });
            }

            var result = await _authService.RegisterAsync(request);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return BadRequest(new { 
                success = false, 
                message = result.Message ?? "Error al registrar usuario" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el registro para usuario: {UserName}", request.UserName);
            return StatusCode(500, new { 
                success = false, 
                message = "Error interno del servidor" 
            });
        }
    }

    /// <summary>
    /// Obtener perfil del usuario. Pasa el token de login como query: ?token=xxx
    /// </summary>
    [HttpGet("profile")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GetProfile([FromQuery] string? token = null)
    {
        if (string.IsNullOrWhiteSpace(token))
            return BadRequest(new { success = false, message = "Parámetro 'token' es requerido (token del login)." });

        var principal = _jwtGenerator.ValidateToken(token);
        var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(new { success = false, message = "Token inválido o expirado." });

        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
            return NotFound(new { success = false, message = "Usuario no encontrado." });

        return Ok(new { success = true, data = user });
    }

    /// <summary>
    /// Verificar estado del servicio de autenticación
    /// </summary>
    /// <returns>Estado del servicio</returns>
    [HttpGet("status")]
    [ProducesResponseType(200)]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            success = true,
            message = "🚀 SIGID Backend API funcionando correctamente!",
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
}