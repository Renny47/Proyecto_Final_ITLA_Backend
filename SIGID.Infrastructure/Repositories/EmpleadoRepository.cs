using Microsoft.EntityFrameworkCore;
using SIGID.Domain.Entities;
using SIGID.Domain.Interfaces;
using SIGID.Infrastructure.Data;

namespace SIGID.Infrastructure.Repositories;

public class EmpleadoRepository : IEmpleadoRepository
{
    private readonly AppDbContext _context;

    public EmpleadoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Empleado?> GetByIdAsync(Guid id)
    {
        return await _context.Empleados
            .Include(e => e.Usuario)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Empleado?> GetByUsuarioIdAsync(string usuarioId)
    {
        return await _context.Empleados
            .Include(e => e.Usuario)
            .FirstOrDefaultAsync(e => e.UsuarioId == usuarioId);
    }

    public async Task<IEnumerable<Empleado>> GetAllAsync()
    {
        return await _context.Empleados
            .Include(e => e.Usuario)
            .OrderBy(e => e.FechaContratacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<Empleado>> GetActivosAsync()
    {
        return await _context.Empleados
            .Include(e => e.Usuario)
            .Where(e => e.Activo)
            .ToListAsync();
    }

    public async Task<IEnumerable<Empleado>> GetByDepartamentoAsync(string departamento)
    {
        return await _context.Empleados
            .Include(e => e.Usuario)
            .Where(e => e.Departamento == departamento)
            .ToListAsync();
    }

    public async Task<Empleado> CreateAsync(Empleado empleado)
    {
        _context.Empleados.Add(empleado);
        await _context.SaveChangesAsync();
        await _context.Entry(empleado).Reference(e => e.Usuario).LoadAsync();
        return empleado;
    }

    public async Task<Empleado> UpdateAsync(Empleado empleado)
    {
        _context.Empleados.Update(empleado);
        await _context.SaveChangesAsync();
        await _context.Entry(empleado).Reference(e => e.Usuario).LoadAsync();
        return empleado;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.Empleados.FindAsync(id);
        if (entity == null) return false;
        _context.Empleados.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Empleados.AnyAsync(e => e.Id == id);
    }

    public async Task<IEnumerable<Empleado>> GetEmpleadosConTurnosAsync(DateTime fecha)
    {
        return await _context.Empleados
            .Include(e => e.Usuario)
            .Where(e => e.Turnos.Any(t => t.FechaTurno.Date == fecha.Date))
            .ToListAsync();
    }
}
