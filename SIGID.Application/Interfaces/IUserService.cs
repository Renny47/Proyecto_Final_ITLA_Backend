using SIGID.Application.DTOs;

namespace SIGID.Application.Interfaces;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(string id);
    Task<UserDto?> GetByUserNameAsync(string userName);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<IEnumerable<UserDto>> GetAllAsync();
    Task<bool> UpdateAsync(string id, UserDto userDto);
    Task<bool> DeactivateAsync(string id);
    Task<bool> ActivateAsync(string id);
}