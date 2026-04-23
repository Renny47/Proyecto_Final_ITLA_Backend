using Microsoft.EntityFrameworkCore;
using SIGID.Domain.Entities;
using SIGID.Domain.Enums;
using SIGID.Domain.Interfaces;
using SIGID.Infrastructure.Data;

namespace SIGID.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UsuarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByIdAsync(string id)
    {
        return await _context.Users
            .Include(u => u.Reservas)
            .Include(u => u.Empleado)
            .Include(u => u.Administrador)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.Reservas)
            .Include(u => u.Empleado)
            .Include(u => u.Administrador)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.Reservas)
            .Include(u => u.Empleado)
            .Include(u => u.Administrador)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Usuario>> GetByTipoAsync(TipoUsuario tipo)
    {
        return await _context.Users
            .Include(u => u.Reservas)
            .Include(u => u.Empleado)
            .Include(u => u.Administrador)
            .Where(u => u.TipoUsuario == tipo)
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }

    public async Task<Usuario> CreateAsync(Usuario usuario)
    {
        _context.Users.Add(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<Usuario> UpdateAsync(Usuario usuario)
    {
        _context.Users.Update(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var usuario = await _context.Users.FindAsync(id);
        if (usuario == null)
            return false;

        _context.Users.Remove(usuario);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id);
    }

    public async Task<IEnumerable<Usuario>> GetClientesConReservasAsync()
    {
        return await _context.Users
            .Include(u => u.Reservas)
            .Where(u => u.TipoUsuario == TipoUsuario.Cliente && u.Reservas.Any())
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync();
    }
}