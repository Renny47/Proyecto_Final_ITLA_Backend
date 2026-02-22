using AutoMapper;
using Microsoft.Extensions.Logging;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;
using SIGID.Domain.Entities;
using SIGID.Domain.Enums;
using SIGID.Domain.Interfaces;

namespace SIGID.Application.Services;

public class InventarioService : IInventarioService
{
    private readonly IInventarioRepository _inventarioRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<InventarioService> _logger;

    public InventarioService(
        IInventarioRepository inventarioRepository,
        IMapper mapper,
        ILogger<InventarioService> logger)
    {
        _inventarioRepository = inventarioRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<InventarioDto?> GetByIdAsync(Guid id)
    {
        var inventario = await _inventarioRepository.GetByIdAsync(id);
        return inventario != null ? _mapper.Map<InventarioDto>(inventario) : null;
    }

    public async Task<IEnumerable<InventarioDto>> GetAllAsync()
    {
        var inventarios = await _inventarioRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<InventarioDto>>(inventarios);
    }

    public async Task<IEnumerable<InventarioDto>> GetByCategoriaAsync(CategoriaInventario categoria)
    {
        var inventarios = await _inventarioRepository.GetByCategoriaAsync(categoria);
        return _mapper.Map<IEnumerable<InventarioDto>>(inventarios);
    }

    public async Task<IEnumerable<InventarioDto>> GetByNivelStockAsync(NivelStock nivel)
    {
        var inventarios = await _inventarioRepository.GetByNivelStockAsync(nivel);
        return _mapper.Map<IEnumerable<InventarioDto>>(inventarios);
    }

    public async Task<IEnumerable<InventarioDto>> GetItemsBajosAsync()
    {
        var inventarios = await _inventarioRepository.GetItemsBajosAsync();
        return _mapper.Map<IEnumerable<InventarioDto>>(inventarios);
    }

    public async Task<IEnumerable<InventarioDto>> GetItemsVencidosAsync()
    {
        var inventarios = await _inventarioRepository.GetItemsVencidosAsync();
        return _mapper.Map<IEnumerable<InventarioDto>>(inventarios);
    }

    public async Task<IEnumerable<InventarioDto>> GetItemsProximosVencerAsync(int dias = 7)
    {
        var inventarios = await _inventarioRepository.GetItemsProximosVencerAsync(dias);
        return _mapper.Map<IEnumerable<InventarioDto>>(inventarios);
    }

    public async Task<InventarioDto> CreateAsync(CreateInventarioDto createInventarioDto)
    {
        var inventario = _mapper.Map<Inventario>(createInventarioDto);
        inventario.Id = Guid.NewGuid();
        inventario.CreatedAt = DateTime.UtcNow;
        inventario.NivelStock = await CalcularNivelStockAsync(
            inventario.CantidadActual, 
            inventario.CantidadMinima, 
            inventario.CantidadMaxima);

        var createdInventario = await _inventarioRepository.CreateAsync(inventario);
        _logger.LogInformation("Inventario creado: {Nombre}, Cantidad: {Cantidad}", 
            createdInventario.Nombre, createdInventario.CantidadActual);
        
        return _mapper.Map<InventarioDto>(createdInventario);
    }

    public async Task<InventarioDto> UpdateAsync(Guid id, UpdateInventarioDto updateInventarioDto)
    {
        var inventario = await _inventarioRepository.GetByIdAsync(id);
        if (inventario == null)
            throw new ArgumentException("Inventario no encontrado");

        _mapper.Map(updateInventarioDto, inventario);
        inventario.UpdatedAt = DateTime.UtcNow;
        inventario.NivelStock = await CalcularNivelStockAsync(
            inventario.CantidadActual, 
            inventario.CantidadMinima, 
            inventario.CantidadMaxima);

        var updatedInventario = await _inventarioRepository.UpdateAsync(inventario);
        return _mapper.Map<InventarioDto>(updatedInventario);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _inventarioRepository.DeleteAsync(id);
    }

    public async Task<InventarioDto> UpdateStockAsync(Guid id, UpdateStockDto updateStockDto)
    {
        var inventarioUpdated = await _inventarioRepository.UpdateStockAsync(id, updateStockDto.NuevaCantidad);
        
        _logger.LogInformation("Stock actualizado: {Nombre}, Nueva cantidad: {Cantidad}, Razón: {Razon}",
            inventarioUpdated.Nombre, updateStockDto.NuevaCantidad, updateStockDto.Razon ?? "No especificada");

        return _mapper.Map<InventarioDto>(inventarioUpdated);
    }

    public async Task<IEnumerable<StockAlertDto>> GetAlertasStockAsync()
    {
        var alertas = new List<StockAlertDto>();

        // Alertas por stock bajo
        var itemsBajos = await _inventarioRepository.GetItemsBajosAsync();
        foreach (var item in itemsBajos)
        {
            alertas.Add(new StockAlertDto
            {
                InventarioId = item.Id,
                Nombre = item.Nombre,
                CantidadActual = item.CantidadActual,
                CantidadMinima = item.CantidadMinima,
                Categoria = item.Categoria.ToString(),
                TipoAlerta = item.NivelStock == NivelStock.Critico ? "Stock Crítico" : "Stock Bajo",
                Mensaje = $"Requiere reposición inmediata. Stock actual: {item.CantidadActual} {item.Unidad}",
                FechaAlerta = DateTime.UtcNow,
                Prioridad = item.NivelStock == NivelStock.Critico ? "Alta" : "Media"
            });
        }

        // Alertas por productos vencidos
        var itemsVencidos = await _inventarioRepository.GetItemsVencidosAsync();
        foreach (var item in itemsVencidos)
        {
            alertas.Add(new StockAlertDto
            {
                InventarioId = item.Id,
                Nombre = item.Nombre,
                CantidadActual = item.CantidadActual,
                CantidadMinima = item.CantidadMinima,
                Categoria = item.Categoria.ToString(),
                TipoAlerta = "Producto Vencido",
                Mensaje = $"Producto vencido desde: {item.FechaVencimiento:dd/MM/yyyy}",
                FechaAlerta = DateTime.UtcNow,
                Prioridad = "Alta"
            });
        }

        // Alertas por productos próximos a vencer
        var itemsProximosVencer = await _inventarioRepository.GetItemsProximosVencerAsync(7);
        foreach (var item in itemsProximosVencer)
        {
            alertas.Add(new StockAlertDto
            {
                InventarioId = item.Id,
                Nombre = item.Nombre,
                CantidadActual = item.CantidadActual,
                CantidadMinima = item.CantidadMinima,
                Categoria = item.Categoria.ToString(),
                TipoAlerta = "Próximo a Vencer",
                Mensaje = $"Vence el: {item.FechaVencimiento:dd/MM/yyyy}",
                FechaAlerta = DateTime.UtcNow,
                Prioridad = "Media"
            });
        }

        return alertas.OrderByDescending(a => a.Prioridad == "Alta" ? 3 : a.Prioridad == "Media" ? 2 : 1)
                     .ThenBy(a => a.FechaAlerta);
    }

    public async Task<NivelStock> CalcularNivelStockAsync(int cantidadActual, int cantidadMinima, int cantidadMaxima)
    {
        await Task.CompletedTask; // Para mantener la signatura async

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