using SIGID.Domain.Entities;
using SIGID.Domain.Enums;

namespace SIGID.Domain.Interfaces;

public interface ITurnoRepository
{
    Task<Turno?> GetByIdAsync(Guid id);
    Task<IEnumerable<Turno>> GetAllAsync();
    Task<IEnumerable<Turno>> GetByEmpleadoAsync(Guid empleadoId);
    Task<IEnumerable<Turno>> GetByFechaAsync(DateTime fecha);
    Task<IEnumerable<Turno>> GetByEstadoAsync(EstadoTurno estado);
    Task<IEnumerable<Turno>> GetTurnosSemanaAsync(DateTime fechaInicio);
    Task<Turno> CreateAsync(Turno turno);
    Task<Turno> UpdateAsync(Turno turno);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<decimal> GetHorasTrabajadasEmpleadoAsync(Guid empleadoId, DateTime fechaInicio, DateTime fechaFin);
}