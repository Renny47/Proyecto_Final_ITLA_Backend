using Microsoft.EntityFrameworkCore;
using SIGID.Domain.Entities;
using SIGID.Domain.Interfaces;
using SIGID.Infrastructure.Data;

namespace SIGID.Infrastructure.Repositories;

public class AdministradorRepository : IAdministradorRepository
{
    private readonly AppDbContext _context;

    public AdministradorRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Administrador?> GetByIdAsync(Guid id)
    {
        return await _context.Administradores
            .Include(a => a.Usuario)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<Administrador?> GetByUsuarioIdAsync(string usuarioId)
    {
        return await _context.Administradores
            .Include(a => a.Usuario)
            .FirstOrDefaultAsync(a => a.UsuarioId == usuarioId);
    }

    public async Task<IEnumerable<Administrador>> GetAllAsync()
    {
        return await _context.Administradores
            .Include(a => a.Usuario)
            .OrderBy(a => a.FechaAsignacion)
            .ToListAsync();
    }

    public async Task<IEnumerable<Administrador>> GetActivosAsync()
    {
        return await _context.Administradores
            .Include(a => a.Usuario)
            .Where(a => a.Activo)
            .ToListAsync();
    }

    public async Task<Administrador> CreateAsync(Administrador administrador)
    {
        _context.Administradores.Add(administrador);
        await _context.SaveChangesAsync();
        await _context.Entry(administrador).Reference(a => a.Usuario).LoadAsync();
        return administrador;
    }

    public async Task<Administrador> UpdateAsync(Administrador administrador)
    {
        _context.Administradores.Update(administrador);
        await _context.SaveChangesAsync();
        await _context.Entry(administrador).Reference(a => a.Usuario).LoadAsync();
        return administrador;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.Administradores.FindAsync(id);
        if (entity == null) return false;
        _context.Administradores.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Administradores.AnyAsync(a => a.Id == id);
    }
}
