using Microsoft.EntityFrameworkCore;
using SIGID.Domain.Entities;
using SIGID.Domain.Enums;
using SIGID.Domain.Interfaces;
using SIGID.Infrastructure.Data;

namespace SIGID.Infrastructure.Repositories;

public class ReservaRepository : IReservaRepository
{
    private readonly AppDbContext _context;

    public ReservaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Reserva?> GetByIdAsync(Guid id)
    {
        return await _context.Reservas
            .Include(r => r.Usuario)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<IEnumerable<Reserva>> GetAllAsync()
    {
        return await _context.Reservas
            .Include(r => r.Usuario)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reserva>> GetByUsuarioAsync(string usuarioId)
    {
        return await _context.Reservas
            .Include(r => r.Usuario)
            .Where(r => r.UsuarioId == usuarioId)
            .OrderByDescending(r => r.FechaHoraReserva)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reserva>> GetByEstadoAsync(EstadoReserva estado)
    {
        return await _context.Reservas
            .Include(r => r.Usuario)
            .Where(r => r.Estado == estado)
            .OrderByDescending(r => r.FechaHoraReserva)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reserva>> GetByFechaRangoAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await _context.Reservas
            .Include(r => r.Usuario)
            .Where(r => r.FechaHoraReserva.Date >= fechaInicio.Date && 
                       r.FechaHoraReserva.Date <= fechaFin.Date)
            .OrderBy(r => r.FechaHoraReserva)
            .ToListAsync();
    }

    public async Task<IEnumerable<Reserva>> GetReservasDelDiaAsync(DateTime fecha)
    {
        return await _context.Reservas
            .Include(r => r.Usuario)
            .Where(r => r.FechaHoraReserva.Date == fecha.Date)
            .OrderBy(r => r.FechaHoraReserva)
            .ToListAsync();
    }

    public async Task<Reserva> CreateAsync(Reserva reserva)
    {
        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();
        
        // Reload with User data
        await _context.Entry(reserva)
            .Reference(r => r.Usuario)
            .LoadAsync();
        
        return reserva;
    }

    public async Task<Reserva> UpdateAsync(Reserva reserva)
    {
        _context.Reservas.Update(reserva);
        await _context.SaveChangesAsync();

        // Reload with User data
        await _context.Entry(reserva)
            .Reference(r => r.Usuario)
            .LoadAsync();

        return reserva;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva == null) return false;

        _context.Reservas.Remove(reserva);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Reservas.AnyAsync(r => r.Id == id);
    }

    public async Task<int> GetTotalPersonasPorDiaAsync(DateTime fecha)
    {
        return await _context.Reservas
            .Where(r => r.FechaHoraReserva.Date == fecha.Date && 
                       (r.Estado == EstadoReserva.Confirmada || r.Estado == EstadoReserva.Pendiente))
            .SumAsync(r => r.NumeroPersonas);
    }

    public async Task<decimal> GetPromedioPersonasPorReservaAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var reservas = await _context.Reservas
            .Where(r => r.FechaHoraReserva.Date >= fechaInicio.Date && 
                       r.FechaHoraReserva.Date <= fechaFin.Date)
            .ToListAsync();

        return reservas.Any() ? (decimal)reservas.Sum(r => r.NumeroPersonas) / reservas.Count : 0;
    }
}