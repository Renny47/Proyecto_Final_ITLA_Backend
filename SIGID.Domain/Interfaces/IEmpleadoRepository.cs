using SIGID.Domain.Entities;

namespace SIGID.Domain.Interfaces;

public interface IEmpleadoRepository
{
    Task<Empleado?> GetByIdAsync(Guid id);
    Task<Empleado?> GetByUsuarioIdAsync(string usuarioId);
    Task<IEnumerable<Empleado>> GetAllAsync();
    Task<IEnumerable<Empleado>> GetActivosAsync();
    Task<IEnumerable<Empleado>> GetByDepartamentoAsync(string departamento);
    Task<Empleado> CreateAsync(Empleado empleado);
    Task<Empleado> UpdateAsync(Empleado empleado);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<IEnumerable<Empleado>> GetEmpleadosConTurnosAsync(DateTime fecha);
}