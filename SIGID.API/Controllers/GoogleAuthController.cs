using Microsoft.AspNetCore.Mvc;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;

namespace SIGID.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class GoogleAuthController : ControllerBase
{
    private readonly IGoogleAuthService _googleAuthService;
    private readonly ILogger<GoogleAuthController> _logger;

    public GoogleAuthController(
        IGoogleAuthService googleAuthService,
        ILogger<GoogleAuthController> logger)
    {
        _googleAuthService = googleAuthService;
        _logger = logger;
    }

    /// <summary>
    /// Obtener URL para iniciar flujo de Google OAuth
    /// </summary>
    /// <returns>URL de autenticación de Google</returns>
    [HttpGet("login-url")]
    [ProducesResponseType(200)]
    public IActionResult GetGoogleLoginUrl()
    {
        try
        {
            var url = _googleAuthService.GetGoogleLoginUrl();
            
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest(new { 
                    success = false, 
                    message = "Google OAuth no está configurado correctamente" 
                });
            }

            return Ok(new { 
                success = true, 
                loginUrl = url,
                message = "URL de Google OAuth generada exitosamente",
                instructions = "1. Abre esta URL en tu navegador\n2. Inicia sesión con Google\n3. Autoriza la aplicación\n4. Copia el código de autorización\n5. Úsalo en el endpoint de login"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar URL de Google OAuth");
            return StatusCode(500, new { 
                success = false, 
                message = "Error interno del servidor" 
            });
        }
    }

    /// <summary>
    /// Manejar callback de Google OAuth y obtener token
    /// </summary>
    /// <param name="code">Código de autorización de Google</param>
    /// <param name="state">Estado para validación CSRF</param>
    /// <returns>Información del callback</returns>
    [HttpGet("callback")]
    [ProducesResponseType(200)]
    public IActionResult GoogleCallback([FromQuery] string code, [FromQuery] string state)
    {
        try
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest(new { 
                    success = false, 
                    message = "Código de autorización no recibido" 
                });
            }

            return Ok(new { 
                success = true, 
                code = code,
                state = state,
                message = "Código de autorización recibido exitosamente",
                nextStep = "Usa este código en un intercambio por token JWT en tu aplicación cliente"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en callback de Google OAuth");
            return StatusCode(500, new { 
                success = false, 
                message = "Error interno del servidor" 
            });
        }
    }

    /// <summary>
    /// Iniciar sesión con token ID de Google
    /// </summary>
    /// <param name="request">Token ID de Google</param>
    /// <returns>Respuesta con token JWT</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto request)
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

            var result = await _googleAuthService.GoogleLoginAsync(request);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return Unauthorized(new { 
                success = false, 
                message = result.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el login con Google");
            return StatusCode(500, new { 
                success = false, 
                message = "Error interno del servidor" 
            });
        }
    }

    /// <summary>    /// Intercambiar código de autorización por token JWT
    /// </summary>
    /// <param name="request">Request con código de autorización de Google</param>
    /// <returns>Token JWT del usuario</returns>
    [HttpPost("exchange-code")]
    [ProducesResponseType(typeof(LoginResponseDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> ExchangeCodeForToken([FromBody] GoogleCodeExchangeRequestDto request)
    {
        try
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(request?.Code))
            {
                return BadRequest(new { 
                    success = false, 
                    message = "Código de autorización es requerido" 
                });
            }

            var result = await _googleAuthService.ExchangeCodeForTokenAsync(request.Code);
            
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return Unauthorized(new { 
                success = false, 
                message = result.Message 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante el intercambio de código");
            return StatusCode(500, new { 
                success = false, 
                message = "Error interno del servidor" 
            });
        }
    }

    /// <summary>    /// Obtener información de configuración de Google OAuth
    /// </summary>
    /// <returns>Información de configuración</returns>
    [HttpGet("info")]
    [ProducesResponseType(200)]
    public IActionResult GetGoogleOAuthInfo()
    {
        return Ok(new
        {
            success = true,
            message = "🔐 Google OAuth configurado correctamente",
            clientId = "169145980043-581aeh1eo6r689u45ck17pesjhkusvuu.apps.googleusercontent.com",
            projectId = "sigid-486103",
            callbackUrl = "http://localhost:5000/api/GoogleAuth/callback",
            instructions = new
            {
                step1 = "Usar GET /api/GoogleAuth/login-url para obtener URL de Google",
                step2 = "Abrir URL en navegador e iniciar sesión con Google", 
                step3 = "Google redirige a /api/GoogleAuth/callback con código",
                step4 = "Usar POST /api/GoogleAuth/exchange-code con el código para obtener JWT",
                step5 = "Usar el JWT en endpoints protegidos con Authorization: Bearer <token>"
            },
            availableEndpoints = new
            {
                getLoginUrl = "GET /api/GoogleAuth/login-url",
                exchangeCode = "POST /api/GoogleAuth/exchange-code ← Recomendado", 
                directLogin = "POST /api/GoogleAuth/login (solo para ID tokens reales de Google)",
                getUserInfo = "GET /api/GoogleAuth/info (este endpoint)"
            }
        });
    }
}