using Microsoft.EntityFrameworkCore;
using SIGID.Domain.Entities;
using SIGID.Domain.Enums;
using SIGID.Domain.Interfaces;
using SIGID.Infrastructure.Data;

namespace SIGID.Infrastructure.Repositories;

public class UserRepository : IUserRepository, IUsuarioRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByIdAsync(string id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<Usuario?> GetByUserNameAsync(string userName)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == userName);
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<Usuario>> GetAllAsync()
    {
        return await _context.Users
            .Where(u => u.IsActive)
            .OrderBy(u => u.UserName)
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
        usuario.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(usuario);
        await _context.SaveChangesAsync();
        return usuario;
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var user = await GetByIdAsync(id);
        if (user == null) return false;

        // Soft delete
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.Users.AnyAsync(u => u.Id == id && u.IsActive);
    }

    public async Task<IEnumerable<Usuario>> GetByTipoAsync(TipoUsuario tipo)
    {
        return await _context.Users
            .Where(u => u.IsActive && u.TipoUsuario == tipo)
            .OrderBy(u => u.UserName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Usuario>> GetClientesConReservasAsync()
    {
        var usuariosConReservas = await _context.Reservas
            .Select(r => r.UsuarioId)
            .Distinct()
            .ToListAsync();
        return await _context.Users
            .Where(u => u.IsActive && usuariosConReservas.Contains(u.Id))
            .OrderBy(u => u.UserName)
            .ToListAsync();
    }
}