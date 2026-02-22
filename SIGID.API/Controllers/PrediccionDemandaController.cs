using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;

namespace SIGID.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PrediccionDemandaController : ControllerBase
{
    private readonly IPrediccionDemandaService _prediccionService;
    private readonly ILogger<PrediccionDemandaController> _logger;

    public PrediccionDemandaController(
        IPrediccionDemandaService prediccionService, 
        ILogger<PrediccionDemandaController> logger)
    {
        _prediccionService = prediccionService;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todas las predicciones
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PrediccionDemandaDto>>> GetAll()
    {
        try
        {
            var predicciones = await _prediccionService.GetAllAsync();
            return Ok(predicciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener predicciones");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener predicción por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<PrediccionDemandaDto>> GetById(Guid id)
    {
        try
        {
            var prediccion = await _prediccionService.GetByIdAsync(id);
            if (prediccion == null)
                return NotFound("Predicción no encontrada");
                
            return Ok(prediccion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener predicción {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener la predicción actual (vigente)
    /// </summary>
    [HttpGet("actual")]
    public async Task<ActionResult<PrediccionDemandaDto>> GetActual()
    {
        try
        {
            var prediccion = await _prediccionService.GetPrediccionActualAsync();
            if (prediccion == null)
                return NotFound("No hay predicción vigente");
                
            return Ok(prediccion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener predicción actual");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener la última predicción generada
    /// </summary>
    [HttpGet("ultima")]
    public async Task<ActionResult<PrediccionDemandaDto>> GetUltima()
    {
        try
        {
            var prediccion = await _prediccionService.GetUltimaPrediccionAsync();
            if (prediccion == null)
                return NotFound("No hay predicciones registradas");
                
            return Ok(prediccion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener última predicción");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// FUNCIONALIDAD CLAVE: Generar nueva predicción de demanda
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<PrediccionDemandaDto>> Create([FromBody] CreatePrediccionDemandaDto createPrediccionDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar rango de fechas
            if (createPrediccionDto.PeriodoFin <= createPrediccionDto.PeriodoInicio)
                return BadRequest("La fecha de fin debe ser posterior a la fecha de inicio");

            if (createPrediccionDto.PeriodoInicio < DateTime.Today)
                return BadRequest("El período de predicción debe ser futuro");

            var prediccion = await _prediccionService.CreatePrediccionAsync(createPrediccionDto);
            return CreatedAtAction(nameof(GetById), new { id = prediccion.Id }, prediccion);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear predicción");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// FUNCIONALIDAD CLAVE: Calcular predicción sin guardar (solo análisis)
    /// </summary>
    [HttpPost("calcular")]
    public async Task<ActionResult<PrediccionResultDto>> CalcularPrediccion([FromBody] CreatePrediccionDemandaDto parametros)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Validar rango de fechas
            if (parametros.PeriodoFin <= parametros.PeriodoInicio)
                return BadRequest("La fecha de fin debe ser posterior a la fecha de inicio");

            var resultado = await _prediccionService.CalcularPrediccionAsync(
                parametros.PeriodoInicio,
                parametros.PeriodoFin,
                parametros.ConsideraFestivos,
                parametros.ConsideraTendencias);
                
            return Ok(resultado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular predicción");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Eliminar predicción
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var resultado = await _prediccionService.DeleteAsync(id);
            if (!resultado)
                return NotFound("Predicción no encontrada");
                
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar predicción {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener métricas históricas - Promedio de reservas diarias
    /// </summary>
    [HttpGet("metricas/reservas-diarias")]
    public async Task<ActionResult<object>> GetPromedioReservasDiarias([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
    {
        try
        {
            if (fechaFin <= fechaInicio)
                return BadRequest("La fecha de fin debe ser posterior a la fecha de inicio");

            var promedio = await _prediccionService.GetPromedioReservasDiariasAsync(fechaInicio, fechaFin);
            return Ok(new 
            { 
                fechaInicio, 
                fechaFin, 
                promedioReservasDiarias = promedio,
                mensaje = $"Promedio de {promedio:F2} reservas por día" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular promedio de reservas diarias");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener métricas históricas - Promedio de personas por reserva
    /// </summary>
    [HttpGet("metricas/personas-por-reserva")]
    public async Task<ActionResult<object>> GetPromedioPersonasPorReserva([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
    {
        try
        {
            if (fechaFin <= fechaInicio)
                return BadRequest("La fecha de fin debe ser posterior a la fecha de inicio");

            var promedio = await _prediccionService.GetPromedioPersonasPorReservaAsync(fechaInicio, fechaFin);
            return Ok(new 
            { 
                fechaInicio, 
                fechaFin, 
                promedioPersonasPorReserva = promedio,
                mensaje = $"Promedio de {promedio:F2} personas por reserva" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular promedio de personas por reserva");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener métricas históricas - Promedio de ingresos por persona
    /// </summary>
    [HttpGet("metricas/ingresos-por-persona")]
    public async Task<ActionResult<object>> GetPromedioIngresosPorPersona([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
    {
        try
        {
            if (fechaFin <= fechaInicio)
                return BadRequest("La fecha de fin debe ser posterior a la fecha de inicio");

            var promedio = await _prediccionService.GetPromedioIngresoPorPersonaAsync(fechaInicio, fechaFin);
            return Ok(new 
            { 
                fechaInicio, 
                fechaFin, 
                promedioIngresoPorPersona = promedio,
                mensaje = $"Promedio de ${promedio:F2} por persona" 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular promedio de ingresos por persona");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener elementos más demandados históricamente
    /// </summary>
    [HttpGet("metricas/elementos-demandados")]
    public async Task<ActionResult<object>> GetElementosMasDemandados([FromQuery] DateTime fechaInicio, [FromQuery] DateTime fechaFin)
    {
        try
        {
            if (fechaFin <= fechaInicio)
                return BadRequest("La fecha de fin debe ser posterior a la fecha de inicio");

            var elementos = await _prediccionService.GetElementosMasDemandadosAsync(fechaInicio, fechaFin);
            return Ok(new 
            { 
                fechaInicio, 
                fechaFin, 
                elementosMasDemandados = elementos,
                totalElementos = elementos.Count 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener elementos más demandados");
            return StatusCode(500, "Error interno del servidor");
        }
    }
}