using Microsoft.EntityFrameworkCore;
using SIGID.Domain.Entities;
using SIGID.Domain.Enums;
using SIGID.Domain.Interfaces;
using SIGID.Infrastructure.Data;

namespace SIGID.Infrastructure.Repositories;

public class InventarioRepository : IInventarioRepository
{
    private readonly AppDbContext _context;

    public InventarioRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Inventario?> GetByIdAsync(Guid id)
    {
        return await _context.Inventarios.FindAsync(id);
    }

    public async Task<IEnumerable<Inventario>> GetAllAsync()
    {
        return await _context.Inventarios
            .OrderBy(i => i.Categoria)
            .ThenBy(i => i.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inventario>> GetByCategoriaAsync(CategoriaInventario categoria)
    {
        return await _context.Inventarios
            .Where(i => i.Categoria == categoria)
            .OrderBy(i => i.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inventario>> GetByNivelStockAsync(NivelStock nivel)
    {
        return await _context.Inventarios
            .Where(i => i.NivelStock == nivel)
            .OrderBy(i => i.CantidadActual)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inventario>> GetItemsBajosAsync()
    {
        return await _context.Inventarios
            .Where(i => i.CantidadActual <= i.CantidadMinima || 
                       i.NivelStock == NivelStock.Bajo || 
                       i.NivelStock == NivelStock.Critico)
            .OrderBy(i => i.CantidadActual)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inventario>> GetItemsVencidosAsync()
    {
        var fechaActual = DateTime.Now.Date;
        return await _context.Inventarios
            .Where(i => i.FechaVencimiento.HasValue && 
                       i.FechaVencimiento.Value.Date < fechaActual)
            .OrderBy(i => i.FechaVencimiento)
            .ToListAsync();
    }

    public async Task<IEnumerable<Inventario>> GetItemsProximosVencerAsync(int dias = 7)
    {
        var fechaLimite = DateTime.Now.Date.AddDays(dias);
        var fechaActual = DateTime.Now.Date;
        
        return await _context.Inventarios
            .Where(i => i.FechaVencimiento.HasValue && 
                       i.FechaVencimiento.Value.Date > fechaActual &&
                       i.FechaVencimiento.Value.Date <= fechaLimite)
            .OrderBy(i => i.FechaVencimiento)
            .ToListAsync();
    }

    public async Task<Inventario> CreateAsync(Inventario inventario)
    {
        _context.Inventarios.Add(inventario);
        await _context.SaveChangesAsync();
        return inventario;
    }

    public async Task<Inventario> UpdateAsync(Inventario inventario)
    {
        inventario.UpdatedAt = DateTime.UtcNow;
        _context.Inventarios.Update(inventario);
        await _context.SaveChangesAsync();
        return inventario;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var inventario = await _context.Inventarios.FindAsync(id);
        if (inventario == null) return false;

        _context.Inventarios.Remove(inventario);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        return await _context.Inventarios.AnyAsync(i => i.Id == id);
    }

    public async Task<Inventario> UpdateStockAsync(Guid id, int nuevaCantidad)
    {
        var inventario = await _context.Inventarios.FindAsync(id);
        if (inventario == null)
            throw new ArgumentException("Inventario no encontrado");

        inventario.CantidadActual = nuevaCantidad;
        inventario.UpdatedAt = DateTime.UtcNow;
        
        // Recalcular nivel de stock
        inventario.NivelStock = CalcularNivelStock(nuevaCantidad, inventario.CantidadMinima, inventario.CantidadMaxima);

        _context.Inventarios.Update(inventario);
        await _context.SaveChangesAsync();
        
        return inventario;
    }

    private NivelStock CalcularNivelStock(int cantidadActual, int cantidadMinima, int cantidadMaxima)
    {
        if (cantidadActual <= 0)
            return NivelStock.Critico;

        if (cantidadActual <= cantidadMinima)
            return NivelStock.Critico;

        if (cantidadActual <= cantidadMinima * 1.5)
            return NivelStock.Bajo;

        if (cantidadActual >= cantidadMaxima * 0.8)
            return NivelStock.Alto;

        return NivelStock.Normal;
    }
}