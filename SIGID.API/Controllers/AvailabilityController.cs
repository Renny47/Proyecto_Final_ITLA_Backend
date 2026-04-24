using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;

namespace SIGID.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AvailabilityController : ControllerBase
{
    private readonly IAvailabilityService _availabilityService;
    private readonly IReservaService _reservaService;
    private readonly ILogger<AvailabilityController> _logger;

    public AvailabilityController(
        IAvailabilityService availabilityService,
        IReservaService reservaService,
        ILogger<AvailabilityController> logger)
    {
        _availabilityService = availabilityService;
        _reservaService = reservaService;
        _logger = logger;
    }

    /// <summary>
    /// Admin crea disponibilidad para una fecha específica
    /// </summary>
    /// <param name="createAvailabilityDto">Datos de la disponibilidad a crear</param>
    /// <returns>Disponibilidad creada</returns>
    [HttpPost]
    //[Authorize(Roles = "Administrador")] // Solo admins pueden crear disponibilidad
    public async Task<ActionResult<AvailabilityDto>> CreateAvailability([FromBody] CreateAvailabilityDto createAvailabilityDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var availability = await _availabilityService.CreateAsync(createAvailabilityDto);
            return CreatedAtAction(nameof(GetAvailabilityById), new { id = availability.Id }, availability);
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
            _logger.LogError(ex, "Error al crear disponibilidad para fecha {Date}", createAvailabilityDto.Date);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Cliente consulta disponibilidad por fecha
    /// </summary>
    /// <param name="date">Fecha a consultar (formato: yyyy-mm-dd)</param>
    /// <returns>Disponibilidad para la fecha especificada</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AvailabilityDto>>> GetAvailability([FromQuery] DateTime date)
    {
        try
        {
            var availability = await _availabilityService.GetByDateAsync(date);
            return Ok(availability);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al consultar disponibilidad para fecha {Date}", date);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener disponibilidad por ID
    /// </summary>
    /// <param name="id">ID de la disponibilidad</param>
    /// <returns>Disponibilidad encontrada</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<AvailabilityDto>> GetAvailabilityById(Guid id)
    {
        try
        {
            var availability = await _availabilityService.GetByIdAsync(id);
            if (availability == null)
                return NotFound("Disponibilidad no encontrada");

            return Ok(availability);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener disponibilidad {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Admin actualiza disponibilidad existente
    /// </summary>
    /// <param name="id">ID de la disponibilidad a actualizar</param>
    /// <param name="updateAvailabilityDto">Nuevos datos de disponibilidad</param>
    /// <returns>Disponibilidad actualizada</returns>
    [HttpPatch("{id}")]
    //[Authorize(Roles = "Administrador")] // Solo admins pueden actualizar
    public async Task<ActionResult<AvailabilityDto>> UpdateAvailability(Guid id, [FromBody] UpdateAvailabilityDto updateAvailabilityDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var availability = await _availabilityService.UpdateAsync(id, updateAvailabilityDto);
            return Ok(availability);
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
            _logger.LogError(ex, "Error al actualizar disponibilidad {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Admin elimina disponibilidad
    /// </summary>
    /// <param name="id">ID de la disponibilidad a eliminar</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("{id}")]
    //[Authorize(Roles = "Administrador")] // Solo admins pueden eliminar
    public async Task<ActionResult> DeleteAvailability(Guid id)
    {
        try
        {
            var resultado = await _availabilityService.DeleteAsync(id);
            if (!resultado)
                return NotFound("Disponibilidad no encontrada");

            return Ok(new { message = "Availability deleted successfully" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar disponibilidad {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ReservationsController : ControllerBase
{
    private readonly IAvailabilityService _availabilityService;
    private readonly IReservaService _reservaService;
    private readonly ILogger<ReservationsController> _logger;

    public ReservationsController(
        IAvailabilityService availabilityService,
        IReservaService reservaService,
        ILogger<ReservationsController> logger)
    {
        _availabilityService = availabilityService;
        _reservaService = reservaService;
        _logger = logger;
    }

    /// <summary>
    /// Cliente crea una reserva usando un time slot disponible
    /// </summary>
    /// <param name="createReservationDto">Datos de la reserva</param>
    /// <returns>Reserva creada</returns>
    [HttpPost]
    [Authorize] // Usuario autenticado requerido
    public async Task<ActionResult<ReservaDto>> CreateReservation([FromBody] CreateReservationDto createReservationDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar que el time slot esté disponible
            var isAvailable = await _availabilityService.ValidateTimeSlotAvailableAsync(createReservationDto.TimeSlotId);
            if (!isAvailable)
            {
                return BadRequest("El horario seleccionado no está disponible o ya está reservado");
            }

            // Crear la reserva tradicional
            var createReservaDto = new CreateReservaDto
            {
                UsuarioId = createReservationDto.UserId,
                FechaHoraReserva = createReservationDto.Date, // Se ajustará con el time slot
                NumeroPersonas = createReservationDto.NumeroPersonas,
                Comentarios = createReservationDto.Comentarios
            };

            var reserva = await _reservaService.CreateAsync(createReservaDto);

            // Marcar el time slot como reservado
            await _availabilityService.BookTimeSlotAsync(createReservationDto.TimeSlotId, reserva.Id);

            return CreatedAtAction("GetById", "Reservas", new { id = reserva.Id }, reserva);
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
            _logger.LogError(ex, "Error al crear reserva para time slot {TimeSlotId}", createReservationDto.TimeSlotId);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Cliente cancela una reserva
    /// </summary>
    /// <param name="id">ID de la reserva a cancelar</param>
    /// <returns>Resultado de la operación</returns>
    [HttpDelete("{id}")]
    [Authorize] // Usuario autenticado requerido
    public async Task<ActionResult> CancelReservation(Guid id)
    {
        try
        {
            // Obtener la reserva para encontrar el TimeSlotId
            var reserva = await _reservaService.GetByIdAsync(id);
            if (reserva == null)
                return NotFound("Reserva no encontrada");

            // Cancelar la reserva
            var resultado = await _reservaService.CancelarReservaAsync(id);
            if (!resultado)
                return NotFound("Reserva no encontrada");

            // Si la reserva tenía un TimeSlot asociado, liberarlo
            // Nota: Necesitaríamos actualizar ReservaDto para incluir TimeSlotId
            // Por ahora, asumimos que se manejará en el servicio

            return Ok(new { message = "Reservation canceled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cancelar reserva {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }
}