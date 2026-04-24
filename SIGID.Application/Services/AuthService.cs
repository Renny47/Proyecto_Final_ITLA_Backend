using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;
using SIGID.Application.Security;
using SIGID.Domain.Entities;
using SIGID.Domain.Exceptions;
using SIGID.Shared.Configuration;

namespace SIGID.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<Usuario> _userManager;
    private readonly IJwtGenerator _jwtGenerator;
    private readonly JwtSettings _jwtSettings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<Usuario> userManager,
        IJwtGenerator jwtGenerator,
        IOptions<JwtSettings> jwtSettings,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _jwtGenerator = jwtGenerator;
        _jwtSettings = jwtSettings?.Value ?? throw new ArgumentNullException(nameof(jwtSettings));
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest)
    {
        try
        {
            // Buscar usuario por email o username
            var user = await _userManager.FindByEmailAsync(loginRequest.UserName) 
                      ?? await _userManager.FindByNameAsync(loginRequest.UserName);
            
            if (user == null)
            {
                return new LoginResponseDto
                {
                    IsSuccess = false,
                    Message = "Usuario o contraseña incorrectos"
                };
            }

            var isValidPassword = await _userManager.CheckPasswordAsync(user, loginRequest.Password);
            if (!isValidPassword)
            {
                return new LoginResponseDto
                {
                    IsSuccess = false,
                    Message = "Usuario o contraseña incorrectos"
                };
            }

            // Actualizar último login
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            var updateResult = await _userManager.UpdateAsync(user);
            
            if (!updateResult.Succeeded)
            {
                _logger.LogWarning("No se pudo actualizar LastLoginAt para usuario {UserName}", user.UserName);
            }

            _logger.LogInformation("Usuario {UserName} autenticado exitosamente", user.UserName);

            var token = _jwtGenerator.GenerateToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(_jwtSettings.TokenExpirationHours);

            return new LoginResponseDto
            {
                IsSuccess = true,
                Message = "Login exitoso",
                Token = token,
                ExpiresAt = expiresAt,
                User = MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en login para usuario {UserName}", loginRequest.UserName);
            throw new AuthenticationException("Error interno durante autenticación");
        }
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto registerRequest)
    {
        try
        {
            var existingUser = await _userManager.FindByNameAsync(registerRequest.UserName);
            if (existingUser != null)
            {
                return new LoginResponseDto
                {
                    IsSuccess = false,
                    Message = "El nombre de usuario ya existe"
                };
            }

            var existingEmail = await _userManager.FindByEmailAsync(registerRequest.Email);
            if (existingEmail != null)
            {
                return new LoginResponseDto
                {
                    IsSuccess = false,
                    Message = "El email ya está registrado"
                };
            }

            var user = new Usuario
            {
                UserName = registerRequest.UserName,
                Email = registerRequest.Email,
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                EmailConfirmed = true, // Para simplicidad
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, registerRequest.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return new LoginResponseDto
                {
                    IsSuccess = false,
                    Message = $"Error al crear usuario: {errors}"
                };
            }

            _logger.LogInformation("Usuario {UserName} registrado exitosamente", user.UserName);

            var token = _jwtGenerator.GenerateToken(user);
            var expiresAt = DateTime.UtcNow.AddHours(_jwtSettings.TokenExpirationHours);

            return new LoginResponseDto
            {
                IsSuccess = true,
                Message = "Usuario registrado exitosamente",
                Token = token,
                ExpiresAt = expiresAt,
                User = MapToUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en registro para usuario {UserName}", registerRequest.UserName);
            return new LoginResponseDto
            {
                IsSuccess = false,
                Message = "Error interno del servidor"
            };
        }
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            return user != null ? MapToUserDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario {UserId}", userId);
            return null;
        }
    }

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
    {
        try
        {
            var users = _userManager.Users.Where(u => u.IsActive).ToList();
            return users.Select(MapToUserDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener lista de usuarios");
            return Enumerable.Empty<UserDto>();
        }
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        // Implementación simple - en producción usar JWT
        return !string.IsNullOrEmpty(token);
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
