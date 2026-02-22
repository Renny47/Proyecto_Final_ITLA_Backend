using Microsoft.EntityFrameworkCore;
using SIGID.Domain.Entities;
using SIGID.Domain.Interfaces;
using SIGID.Infrastructure.Data;

namespace SIGID.Infrastructure.Repositories;

public class PrediccionDemandaRepository : IPrediccionDemandaRepository
{
    private readonly AppDbContext _context;

    public PrediccionDemandaRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PrediccionDemanda?> GetByIdAsync(Guid id)
    {
        return await _context.PrediccionesDemanda.FindAsync(id);
    }

    public async Task<IEnumerable<PrediccionDemanda>> GetAllAsync()
    {
        return await _context.PrediccionesDemanda
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<PrediccionDemanda?> GetPrediccionActualAsync()
    {
        var now = DateTime.UtcNow;
        return await _context.PrediccionesDemanda
            .FirstOrDefaultAsync(p => p.PeriodoInicio <= now && p.PeriodoFin >= now);
    }

    public async Task<IEnumerable<PrediccionDemanda>> GetPrediccionesPorRangoAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        return await _context.PrediccionesDemanda
            .Where(p => p.PeriodoInicio >= fechaInicio && p.PeriodoFin <= fechaFin)
            .OrderBy(p => p.PeriodoInicio)
            .ToListAsync();
    }

    public async Task<PrediccionDemanda?> GetUltimaPrediccionAsync()
    {
        return await _context.PrediccionesDemanda
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<PrediccionDemanda> CreateAsync(PrediccionDemanda prediccion)
    {
        _context.PrediccionesDemanda.Add(prediccion);
        await _context.SaveChangesAsync();
        return prediccion;
    }

    public async Task<PrediccionDemanda> UpdateAsync(PrediccionDemanda prediccion)
    {
        _context.PrediccionesDemanda.Update(prediccion);
        await _context.SaveChangesAsync();
        return prediccion;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.PrediccionesDemanda.FindAsync(id);
        if (entity == null) return false;
        _context.PrediccionesDemanda.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.PrediccionesDemanda.AnyAsync(p => p.Id == id);
    }
}
