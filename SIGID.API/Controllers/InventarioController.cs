using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;
using SIGID.Domain.Enums;

namespace SIGID.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class InventarioController : ControllerBase
{
    private readonly IInventarioService _inventarioService;
    private readonly ILogger<InventarioController> _logger;

    public InventarioController(IInventarioService inventarioService, ILogger<InventarioController> logger)
    {
        _inventarioService = inventarioService;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todo el inventario
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventarioDto>>> GetAll()
    {
        try
        {
            var inventario = await _inventarioService.GetAllAsync();
            return Ok(inventario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener inventario");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener item de inventario por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<InventarioDto>> GetById(Guid id)
    {
        try
        {
            var item = await _inventarioService.GetByIdAsync(id);
            if (item == null)
                return NotFound("Item de inventario no encontrado");
                
            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener item de inventario {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener inventario por categoría
    /// </summary>
    [HttpGet("categoria/{categoria}")]
    public async Task<ActionResult<IEnumerable<InventarioDto>>> GetByCategoria(CategoriaInventario categoria)
    {
        try
        {
            var items = await _inventarioService.GetByCategoriaAsync(categoria);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener inventario por categoría {Categoria}", categoria);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener items con stock bajo
    /// </summary>
    [HttpGet("stock-bajo")]
    public async Task<ActionResult<IEnumerable<InventarioDto>>> GetItemsBajos()
    {
        try
        {
            var items = await _inventarioService.GetItemsBajosAsync();
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener items con stock bajo");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener items vencidos
    /// </summary>
    [HttpGet("vencidos")]
    public async Task<ActionResult<IEnumerable<InventarioDto>>> GetItemsVencidos()
    {
        try
        {
            var items = await _inventarioService.GetItemsVencidosAsync();
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener items vencidos");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener items próximos a vencer
    /// </summary>
    [HttpGet("proximos-vencer")]
    public async Task<ActionResult<IEnumerable<InventarioDto>>> GetItemsProximosVencer([FromQuery] int dias = 7)
    {
        try
        {
            var items = await _inventarioService.GetItemsProximosVencerAsync(dias);
            return Ok(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener items próximos a vencer en {Dias} días", dias);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// FUNCIONALIDAD CLAVE: Obtener alertas de stock
    /// </summary>
    [HttpGet("alertas")]
    public async Task<ActionResult<IEnumerable<StockAlertDto>>> GetAlertasStock()
    {
        try
        {
            var alertas = await _inventarioService.GetAlertasStockAsync();
            return Ok(alertas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener alertas de stock");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crear nuevo item de inventario
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<InventarioDto>> Create([FromBody] CreateInventarioDto createInventarioDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var item = await _inventarioService.CreateAsync(createInventarioDto);
            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear item de inventario");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualizar item de inventario
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<InventarioDto>> Update(Guid id, [FromBody] UpdateInventarioDto updateInventarioDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var item = await _inventarioService.UpdateAsync(id, updateInventarioDto);
            return Ok(item);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar item de inventario {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// FUNCIONALIDAD CLAVE: Actualizar stock específico
    /// </summary>
    [HttpPatch("{id}/stock")]
    public async Task<ActionResult<InventarioDto>> UpdateStock(Guid id, [FromBody] UpdateStockDto updateStockDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var item = await _inventarioService.UpdateStockAsync(id, updateStockDto);
            return Ok(item);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar stock del item {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Eliminar item de inventario
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var resultado = await _inventarioService.DeleteAsync(id);
            if (!resultado)
                return NotFound("Item de inventario no encontrado");
                
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar item de inventario {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}