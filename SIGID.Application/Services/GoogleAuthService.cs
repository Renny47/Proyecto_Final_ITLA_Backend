using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;
using SIGID.Application.Security;
using SIGID.Domain.Entities;
using SIGID.Shared.Configuration;

namespace SIGID.Application.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(
        UserManager<Usuario> userManager,
        IConfiguration configuration,
        IJwtGenerator jwtGenerator,
        IOptions<JwtSettings> jwtSettings,
        ILogger<GoogleAuthService> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _jwtGenerator = jwtGenerator;
        _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        _logger = logger;
    }

    public async Task<LoginResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request)
    {
        try
        {
            var googleClientId = _configuration["GoogleAuth:ClientId"];
            
            if (string.IsNullOrEmpty(googleClientId))
            {
                return new LoginResponseDto 
                { 
                    IsSuccess = false, 
                    Message = "Google OAuth no está configurado" 
                };
            }

            // Verificar el token de Google
            var payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, new GoogleJsonWebSignature.ValidationSettings()
            {
                Audience = new[] { googleClientId }
            });

            if (payload == null)
            {
                return new LoginResponseDto 
                { 
                    IsSuccess = false, 
                    Message = "Token de Google inválido" 
                };
            }

            // Validar que el email esté verificado
            if (!payload.EmailVerified)
            {
                return new LoginResponseDto 
                { 
                    IsSuccess = false, 
                    Message = "El email de Google debe estar verificado" 
                };
            }

            // Buscar o crear usuario
            var user = await _userManager.FindByEmailAsync(payload.Email);
            
            if (user == null)
            {
                // Crear nuevo usuario
                user = new Usuario
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    EmailConfirmed = payload.EmailVerified,
                    FirstName = payload.GivenName ?? "",
                    LastName = payload.FamilyName ?? "",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow
                };

                var createResult = await _userManager.CreateAsync(user);
                
                if (!createResult.Succeeded)
                {
                    _logger.LogError("Error creating user from Google OAuth: {Errors}", 
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    
                    return new LoginResponseDto 
                    { 
                        IsSuccess = false, 
                        Message = "Error al crear usuario con Google OAuth" 
                    };
                }

                // Asignar rol por defecto "Cliente" a usuarios de Google OAuth
                await _userManager.AddToRoleAsync(user, "Cliente");

                _logger.LogInformation("Usuario creado exitosamente desde Google OAuth: {Email}", payload.Email);
            }
            else
            {
                // Actualizar información del usuario existente
                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                
                if (!user.EmailConfirmed && payload.EmailVerified)
                {
                    user.EmailConfirmed = true;
                }

                await _userManager.UpdateAsync(user);
                _logger.LogInformation("Usuario autenticado exitosamente con Google OAuth: {Email}", payload.Email);
            }

            // Generar JWT token
            var token = _jwtGenerator.GenerateToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(_jwtSettings.TokenExpirationHours);

            return new LoginResponseDto
            {
                IsSuccess = true,
                Token = token,
                Message = "Autenticación con Google exitosa",
                ExpiresAt = expiresAt,
                User = MapToUserDto(user) // Incluir información del usuario
            };
        }
        catch (InvalidJwtException)
        {
            _logger.LogWarning("Token de Google inválido recibido");
            return new LoginResponseDto 
            { 
                IsSuccess = false, 
                Message = "Token de Google inválido" 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la autenticación con Google OAuth");
            return new LoginResponseDto 
            { 
                IsSuccess = false, 
                Message = "Error interno del servidor" 
            };
        }
    }

    private static UserDto MapToUserDto(Usuario user)
    {
        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? "",
            Email = user.Email ?? "",
            FirstName = user.FirstName,
            LastName = user.LastName,
            EmailConfirmed = user.EmailConfirmed,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}