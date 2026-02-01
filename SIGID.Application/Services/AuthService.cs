using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;
using SIGID.Domain.Entities;
using SIGID.Domain.Exceptions;

namespace SIGID.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<User> userManager,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto loginRequest)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(loginRequest.UserName);
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
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("Usuario {UserName} autenticado exitosamente", user.UserName);

            return new LoginResponseDto
            {
                IsSuccess = true,
                Message = "Login exitoso",
                Token = "simple-token", // En arquitectura simple
                ExpiresAt = DateTime.UtcNow.AddHours(24),
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

            var user = new User
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

            return new LoginResponseDto
            {
                IsSuccess = true,
                Message = "Usuario registrado exitosamente",
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

    private static UserDto MapToUserDto(User user)
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
