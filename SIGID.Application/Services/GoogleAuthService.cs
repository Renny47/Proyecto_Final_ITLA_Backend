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
                _logger.LogInformation("Creando nuevo usuario desde Google OAuth: {Email}", payload.Email);
                
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
                    LastLoginAt = DateTime.UtcNow,
                    TipoUsuario = Domain.Enums.TipoUsuario.Cliente
                };

                // Crear usuario con contraseña aleatoria (no se usará, solo para cumplir con Identity)
                var randomPassword = GenerateRandomPassword();
                var createResult = await _userManager.CreateAsync(user, randomPassword);
                
                if (!createResult.Succeeded)
                {
                    _logger.LogError("Error creating user from Google OAuth: {Errors}", 
                        string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    
                    return new LoginResponseDto 
                    { 
                        IsSuccess = false, 
                        Message = $"Error al crear usuario: {string.Join(", ", createResult.Errors.Select(e => e.Description))}" 
                    };
                }

                // Asignar rol por defecto "Cliente" a usuarios de Google OAuth
                var roleResult = await _userManager.AddToRoleAsync(user, "Cliente");
                if (!roleResult.Succeeded)
                {
                    _logger.LogWarning("No se pudo asignar rol Cliente a usuario {Email}: {Errors}", 
                        payload.Email, string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }

                _logger.LogInformation("Usuario creado exitosamente desde Google OAuth: {Email} con ID: {UserId}", payload.Email, user.Id);
            }
            else
            {
                _logger.LogInformation("Usuario existente encontrado, actualizando login: {Email}", payload.Email);
                
                // Actualizar información del usuario existente
                user.LastLoginAt = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                
                if (!user.EmailConfirmed && payload.EmailVerified)
                {
                    user.EmailConfirmed = true;
                }

                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    _logger.LogWarning("No se pudo actualizar usuario {Email}: {Errors}", 
                        payload.Email, string.Join(", ", updateResult.Errors.Select(e => e.Description)));
                }
                else
                {
                    _logger.LogInformation("Usuario actualizado exitosamente: {Email}", payload.Email);
                }
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

    private static string GenerateRandomPassword()
    {
        // Generar contraseña aleatoria segura para usuarios de Google OAuth
        // Esta contraseña no se usará nunca, solo cumple con los requisitos de Identity
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
        var random = new Random();
        var password = new string(Enumerable.Repeat(chars, 16)
            .Select(s => s[random.Next(s.Length)]).ToArray());
        
        return password + "A1!"; // Asegurar que cumple con todos los requisitos
    }
}