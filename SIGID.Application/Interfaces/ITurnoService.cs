using SIGID.Application.DTOs;
using SIGID.Domain.Enums;

namespace SIGID.Application.Interfaces;

public interface ITurnoService
{
    Task<TurnoDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<TurnoDto>> GetAllAsync();
    Task<IEnumerable<TurnoDto>> GetByEmpleadoAsync(Guid empleadoId);
    Task<IEnumerable<TurnoDto>> GetByFechaAsync(DateTime fecha);
    Task<IEnumerable<TurnoDto>> GetTurnosSemanaAsync(DateTime fechaInicio);
    Task<TurnoDto> CreateAsync(CreateTurnoDto createTurnoDto);
    Task<TurnoDto> UpdateAsync(Guid id, UpdateTurnoDto updateTurnoDto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> IniciarTurnoAsync(Guid id);
    Task<bool> FinalizarTurnoAsync(Guid id);
    Task<decimal> GetHorasTrabajadasEmpleadoAsync(Guid empleadoId, DateTime fechaInicio, DateTime fechaFin);
}