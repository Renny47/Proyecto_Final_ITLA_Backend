using SIGID.Application.DTOs;

namespace SIGID.Application.Interfaces;

public interface IEmpleadoService
{
    Task<EmpleadoDto?> GetByIdAsync(Guid id);
    Task<EmpleadoDto?> GetByUsuarioIdAsync(string usuarioId);
    Task<IEnumerable<EmpleadoDto>> GetAllAsync();
    Task<IEnumerable<EmpleadoDto>> GetActivosAsync();
    Task<IEnumerable<EmpleadoDto>> GetByDepartamentoAsync(string departamento);
    Task<EmpleadoDto> CreateAsync(CreateEmpleadoDto createEmpleadoDto);
    Task<EmpleadoDto> UpdateAsync(Guid id, EmpleadoDto empleadoDto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<EmpleadoDto>> GetEmpleadosConTurnosAsync(DateTime fecha);
    Task<IEnumerable<TurnoDto>> GetTurnosEmpleadoAsync(Guid empleadoId, DateTime fechaInicio, DateTime fechaFin);
}