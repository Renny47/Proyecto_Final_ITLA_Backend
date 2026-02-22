using SIGID.Application.DTOs;
using SIGID.Domain.Enums;

namespace SIGID.Application.Interfaces;

public interface IUsuarioService
{
    Task<UsuarioDto?> GetByIdAsync(string id);
    Task<UsuarioDto?> GetByEmailAsync(string email);
    Task<IEnumerable<UsuarioDto>> GetAllAsync();
    Task<IEnumerable<UsuarioDto>> GetByTipoAsync(TipoUsuario tipo);
    Task<UsuarioDto> CreateAsync(UsuarioDto usuarioDto);
    Task<UsuarioDto> UpdateAsync(string id, UsuarioDto usuarioDto);
    Task<bool> DeleteAsync(string id);
    Task<IEnumerable<UsuarioDto>> GetClientesConReservasAsync();
}