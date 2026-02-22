using SIGID.Domain.Entities;
using SIGID.Domain.Enums;

namespace SIGID.Domain.Interfaces;

public interface IReservaRepository
{
    Task<Reserva?> GetByIdAsync(Guid id);
    Task<IEnumerable<Reserva>> GetAllAsync();
    Task<IEnumerable<Reserva>> GetByUsuarioAsync(string usuarioId);
    Task<IEnumerable<Reserva>> GetByEstadoAsync(EstadoReserva estado);
    Task<IEnumerable<Reserva>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<IEnumerable<Reserva>> GetReservasDelDiaAsync(DateTime fecha);
    Task<Reserva> CreateAsync(Reserva reserva);
    Task<Reserva> UpdateAsync(Reserva reserva);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<int> GetTotalPersonasPorDiaAsync(DateTime fecha);
    Task<decimal> GetPromedioPersonasPorReservaAsync(DateTime fechaInicio, DateTime fechaFin);
}