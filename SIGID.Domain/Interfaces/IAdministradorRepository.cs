using SIGID.Domain.Entities;

namespace SIGID.Domain.Interfaces;

public interface IAdministradorRepository
{
    Task<Administrador?> GetByIdAsync(Guid id);
    Task<Administrador?> GetByUsuarioIdAsync(string usuarioId);
    Task<IEnumerable<Administrador>> GetAllAsync();
    Task<IEnumerable<Administrador>> GetActivosAsync();
    Task<Administrador> CreateAsync(Administrador administrador);
    Task<Administrador> UpdateAsync(Administrador administrador);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}