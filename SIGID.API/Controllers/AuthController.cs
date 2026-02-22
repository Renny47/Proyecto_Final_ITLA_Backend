using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;

namespace SIGID.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ILogger<AuthController> logger)
    {
        _authService = authService;
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
    /// Consultar perfil: con token devuelve el usuario autenticado; sin token devuelve que no hay sesión (sin restricción de acceso).
    /// </summary>
    [HttpGet("profile")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(UserDto), 200)]
    [ProducesResponseType(200)]
    public async Task<IActionResult> GetProfile()
    {
        try
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Ok(new
                {
                    success = true,
                    authenticated = false,
                    message = "No hay sesión activa. Envía el header Authorization: Bearer <token> para ver tu perfil.",
                    data = (object?)null
                });
            }

            var user = await _authService.GetUserByIdAsync(userId);

            if (user == null)
            {
                return Ok(new
                {
                    success = true,
                    authenticated = false,
                    message = "Usuario no encontrado",
                    data = (object?)null
                });
            }

            return Ok(new
            {
                success = true,
                authenticated = true,
                data = user
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener perfil de usuario");
            return StatusCode(500, new
            {
                success = false,
                message = "Error interno del servidor"
            });
        }
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