using SIGID.Application.DTOs;
using SIGID.Domain.Enums;

namespace SIGID.Application.Interfaces;

public interface IInventarioService
{
    Task<InventarioDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<InventarioDto>> GetAllAsync();
    Task<IEnumerable<InventarioDto>> GetByCategoriaAsync(CategoriaInventario categoria);
    Task<IEnumerable<InventarioDto>> GetByNivelStockAsync(NivelStock nivel);
    Task<IEnumerable<InventarioDto>> GetItemsBajosAsync();
    Task<IEnumerable<InventarioDto>> GetItemsVencidosAsync();
    Task<IEnumerable<InventarioDto>> GetItemsProximosVencerAsync(int dias = 7);
    Task<InventarioDto> CreateAsync(CreateInventarioDto createInventarioDto);
    Task<InventarioDto> UpdateAsync(Guid id, UpdateInventarioDto updateInventarioDto);
    Task<bool> DeleteAsync(Guid id);
    Task<InventarioDto> UpdateStockAsync(Guid id, UpdateStockDto updateStockDto);
    Task<IEnumerable<StockAlertDto>> GetAlertasStockAsync();
    Task<NivelStock> CalcularNivelStockAsync(int cantidadActual, int cantidadMinima, int cantidadMaxima);
}