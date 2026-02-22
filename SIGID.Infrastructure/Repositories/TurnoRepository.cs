using Microsoft.EntityFrameworkCore;
using SIGID.Domain.Entities;
using SIGID.Domain.Enums;
using SIGID.Domain.Interfaces;
using SIGID.Infrastructure.Data;

namespace SIGID.Infrastructure.Repositories;

public class TurnoRepository : ITurnoRepository
{
    private readonly AppDbContext _context;

    public TurnoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Turno?> GetByIdAsync(Guid id)
    {
        return await _context.Turnos
            .Include(t => t.Empleado)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Turno>> GetAllAsync()
    {
        return await _context.Turnos
            .Include(t => t.Empleado)
            .OrderByDescending(t => t.FechaTurno)
            .ThenBy(t => t.HoraInicio)
            .ToListAsync();
    }

    public async Task<IEnumerable<Turno>> GetByEmpleadoAsync(Guid empleadoId)
    {
        return await _context.Turnos
            .Include(t => t.Empleado)
            .Where(t => t.EmpleadoId == empleadoId)
            .OrderByDescending(t => t.FechaTurno)
            .ToListAsync();
    }

    public async Task<IEnumerable<Turno>> GetByFechaAsync(DateTime fecha)
    {
        var date = fecha.Date;
        return await _context.Turnos
            .Include(t => t.Empleado)
            .Where(t => t.FechaTurno.Date == date)
            .OrderBy(t => t.HoraInicio)
            .ToListAsync();
    }

    public async Task<IEnumerable<Turno>> GetByEstadoAsync(EstadoTurno estado)
    {
        return await _context.Turnos
            .Include(t => t.Empleado)
            .Where(t => t.Estado == estado)
            .OrderByDescending(t => t.FechaTurno)
            .ToListAsync();
    }

    public async Task<IEnumerable<Turno>> GetTurnosSemanaAsync(DateTime fechaInicio)
    {
        var fechaFin = fechaInicio.Date.AddDays(7);
        return await _context.Turnos
            .Include(t => t.Empleado)
            .Where(t => t.FechaTurno.Date >= fechaInicio.Date && t.FechaTurno.Date < fechaFin)
            .OrderBy(t => t.FechaTurno)
            .ThenBy(t => t.HoraInicio)
            .ToListAsync();
    }

    public async Task<Turno> CreateAsync(Turno turno)
    {
        _context.Turnos.Add(turno);
        await _context.SaveChangesAsync();
        await _context.Entry(turno).Reference(t => t.Empleado).LoadAsync();
        return turno;
    }

    public async Task<Turno> UpdateAsync(Turno turno)
    {
        _context.Turnos.Update(turno);
        await _context.SaveChangesAsync();
        await _context.Entry(turno).Reference(t => t.Empleado).LoadAsync();
        return turno;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.Turnos.FindAsync(id);
        if (entity == null) return false;
        _context.Turnos.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Turnos.AnyAsync(t => t.Id == id);
    }

    public async Task<decimal> GetHorasTrabajadasEmpleadoAsync(Guid empleadoId, DateTime fechaInicio, DateTime fechaFin)
    {
        var total = await _context.Turnos
            .Where(t => t.EmpleadoId == empleadoId &&
                        t.FechaTurno.Date >= fechaInicio.Date &&
                        t.FechaTurno.Date <= fechaFin.Date &&
                        t.HorasTrabajadas != null)
            .SumAsync(t => t.HorasTrabajadas ?? 0);
        return total;
    }
}
