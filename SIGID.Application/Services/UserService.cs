using Microsoft.AspNetCore.Identity;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;
using SIGID.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace SIGID.Application.Services;

public class UserService : IUserService
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UserService> _logger;

    public UserService(UserManager<User> userManager, ILogger<UserService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<UserDto?> GetByIdAsync(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            return user != null ? MapToUserDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario por ID {Id}", id);
            return null;
        }
    }

    public async Task<UserDto?> GetByUserNameAsync(string userName)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(userName);
            return user != null ? MapToUserDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario por UserName {UserName}", userName);
            return null;
        }
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user != null ? MapToUserDto(user) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario por Email {Email}", email);
            return null;
        }
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
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

    public async Task<bool> UpdateAsync(string id, UserDto userDto)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Email = userDto.Email;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar usuario {Id}", id);
            return false;
        }
    }

    public async Task<bool> DeactivateAsync(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al desactivar usuario {Id}", id);
            return false;
        }
    }

    public async Task<bool> ActivateAsync(string id)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return false;

            user.IsActive = true;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al activar usuario {Id}", id);
            return false;
        }
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