using SIGID.Application.DTOs;
using SIGID.Domain.Enums;

namespace SIGID.Application.Interfaces;

public interface IReservaService
{
    Task<ReservaDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ReservaDto>> GetAllAsync();
    Task<IEnumerable<ReservaDto>> GetByUsuarioAsync(string usuarioId);
    Task<IEnumerable<ReservaDto>> GetByEstadoAsync(EstadoReserva estado);
    Task<IEnumerable<ReservaDto>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<IEnumerable<ReservaDto>> GetReservasDelDiaAsync(DateTime fecha);
    Task<ReservaDto> CreateAsync(CreateReservaDto createReservaDto);
    Task<ReservaDto> UpdateAsync(Guid id, UpdateReservaDto updateReservaDto);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ConfirmarReservaAsync(Guid id);
    Task<bool> CancelarReservaAsync(Guid id);
    Task<int> GetCapacidadDisponibleAsync(DateTime fecha, TimeSpan hora);
}