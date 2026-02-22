using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;
using SIGID.Application.Security;
using SIGID.Domain.Entities;
using SIGID.Domain.Enums;

namespace SIGID.Application.Services;

public class GoogleAuthService : IGoogleAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly ILogger<GoogleAuthService> _logger;

    public GoogleAuthService(
        UserManager<User> userManager,
        IConfiguration configuration,
        IJwtGenerator jwtGenerator,
        ILogger<GoogleAuthService> logger)
    {
        _userManager = userManager;
        _configuration = configuration;
        _jwtGenerator = jwtGenerator;
        _logger = logger;
    }

    public async Task<LoginResponseDto> GoogleLoginAsync(GoogleLoginRequestDto request)
    {
        try
        {
            var googleClientId = _configuration["GoogleAuth:ClientId"];
            _logger.LogInformation("ClientId loaded: {ClientId}", googleClientId);
            
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
                user = new User
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

            return new LoginResponseDto
            {
                IsSuccess = true,
                Token = token,
                Message = "Autenticación con Google exitosa"
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
}