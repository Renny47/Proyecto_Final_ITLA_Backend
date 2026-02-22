using SIGID.Domain.Entities;
using SIGID.Domain.Enums;

namespace SIGID.Domain.Interfaces;

public interface IInventarioRepository
{
    Task<Inventario?> GetByIdAsync(Guid id);
    Task<IEnumerable<Inventario>> GetAllAsync();
    Task<IEnumerable<Inventario>> GetByCategoriaAsync(CategoriaInventario categoria);
    Task<IEnumerable<Inventario>> GetByNivelStockAsync(NivelStock nivel);
    Task<IEnumerable<Inventario>> GetItemsBajosAsync(); // Stock bajo o crítico
    Task<IEnumerable<Inventario>> GetItemsVencidosAsync();
    Task<IEnumerable<Inventario>> GetItemsProximosVencerAsync(int dias = 7);
    Task<Inventario> CreateAsync(Inventario inventario);
    Task<Inventario> UpdateAsync(Inventario inventario);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<Inventario> UpdateStockAsync(Guid id, int nuevaCantidad);
}