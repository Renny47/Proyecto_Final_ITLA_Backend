using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;
using SIGID.Domain.Enums;

namespace SIGID.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReservasController : ControllerBase
{
    private readonly IReservaService _reservaService;
    private readonly ILogger<ReservasController> _logger;

    public ReservasController(IReservaService reservaService, ILogger<ReservasController> logger)
    {
        _reservaService = reservaService;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todas las reservas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservaDto>>> GetAll()
    {
        try
        {
            var reservas = await _reservaService.GetAllAsync();
            return Ok(reservas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reservas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener reserva por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ReservaDto>> GetById(Guid id)
    {
        try
        {
            var reserva = await _reservaService.GetByIdAsync(id);
            if (reserva == null)
                return NotFound("Reserva no encontrada");
                
            return Ok(reserva);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reserva {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener reservas por usuario
    /// </summary>
    [HttpGet("usuario/{usuarioId}")]
    public async Task<ActionResult<IEnumerable<ReservaDto>>> GetByUsuario(string usuarioId)
    {
        try
        {
            var reservas = await _reservaService.GetByUsuarioAsync(usuarioId);
            return Ok(reservas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reservas del usuario {UsuarioId}", usuarioId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener reservas por estado
    /// </summary>
    [HttpGet("estado/{estado}")]
    public async Task<ActionResult<IEnumerable<ReservaDto>>> GetByEstado(EstadoReserva estado)
    {
        try
        {
            var reservas = await _reservaService.GetByEstadoAsync(estado);
            return Ok(reservas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reservas por estado {Estado}", estado);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener reservas del día
    /// </summary>
    [HttpGet("dia/{fecha}")]
    public async Task<ActionResult<IEnumerable<ReservaDto>>> GetReservasDelDia(DateTime fecha)
    {
        try
        {
            var reservas = await _reservaService.GetReservasDelDiaAsync(fecha);
            return Ok(reservas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reservas del día {Fecha}", fecha);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crear nueva reserva
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ReservaDto>> Create([FromBody] CreateReservaDto createReservaDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reserva = await _reservaService.CreateAsync(createReservaDto);
            return CreatedAtAction(nameof(GetById), new { id = reserva.Id }, reserva);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear reserva");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualizar reserva
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ReservaDto>> Update(Guid id, [FromBody] UpdateReservaDto updateReservaDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var reserva = await _reservaService.UpdateAsync(id, updateReservaDto);
            return Ok(reserva);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar reserva {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Eliminar reserva
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var resultado = await _reservaService.DeleteAsync(id);
            if (!resultado)
                return NotFound("Reserva no encontrada");
                
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar reserva {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Confirmar reserva
    /// </summary>
    [HttpPatch("{id}/confirmar")]
    public async Task<ActionResult> ConfirmarReserva(Guid id)
    {
        try
        {
            var resultado = await _reservaService.ConfirmarReservaAsync(id);
            if (!resultado)
                return NotFound("Reserva no encontrada");
                
            return Ok(new { message = "Reserva confirmada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al confirmar reserva {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Cancelar reserva
    /// </summary>
    [HttpPatch("{id}/cancelar")]
    public async Task<ActionResult> CancelarReserva(Guid id)
    {
        try
        {
            var resultado = await _reservaService.CancelarReservaAsync(id);
            if (!resultado)
                return NotFound("Reserva no encontrada");
                
            return Ok(new { message = "Reserva cancelada exitosamente" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar reserva {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Verificar capacidad disponible
    /// </summary>
    [HttpGet("capacidad")]
    public async Task<ActionResult<object>> GetCapacidadDisponible([FromQuery] DateTime fecha, [FromQuery] TimeSpan hora)
    {
        try
        {
            var capacidad = await _reservaService.GetCapacidadDisponibleAsync(fecha, hora);
            return Ok(new { fecha, hora, capacidadDisponible = capacidad });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar capacidad para {Fecha} a las {Hora}", fecha, hora);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}