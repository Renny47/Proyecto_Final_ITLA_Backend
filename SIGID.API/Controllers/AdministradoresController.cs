using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;

namespace SIGID.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AdministradoresController : ControllerBase
{
    private readonly IAdministradorService _administradorService;
    private readonly ILogger<AdministradoresController> _logger;

    public AdministradoresController(
        IAdministradorService administradorService, 
        ILogger<AdministradoresController> logger)
    {
        _administradorService = administradorService;
        _logger = logger;
    }

    /// <summary>
    /// Obtener todos los administradores
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AdministradorDto>>> GetAll()
    {
        try
        {
            var administradores = await _administradorService.GetAllAsync();
            return Ok(administradores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener administradores");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener administrador por ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<AdministradorDto>> GetById(Guid id)
    {
        try
        {
            var administrador = await _administradorService.GetByIdAsync(id);
            if (administrador == null)
                return NotFound("Administrador no encontrado");
                
            return Ok(administrador);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener administrador {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Crear nuevo administrador
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<AdministradorDto>> Create([FromBody] CreateAdministradorDto createAdministradorDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var administrador = await _administradorService.CreateAsync(createAdministradorDto);
            return CreatedAtAction(nameof(GetById), new { id = administrador.Id }, administrador);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear administrador");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Actualizar administrador existente
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<AdministradorDto>> Update(Guid id, [FromBody] UpdateAdministradorDto updateAdministradorDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var administrador = await _administradorService.UpdateAsync(id, updateAdministradorDto);
            if (administrador == null)
                return NotFound("Administrador no encontrado");
                
            return Ok(administrador);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar administrador {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Eliminar administrador
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(Guid id)
    {
        try
        {
            var resultado = await _administradorService.DeleteAsync(id);
            if (!resultado)
                return NotFound("Administrador no encontrado");
                
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar administrador {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Obtener administradores activos solamente
    /// </summary>
    [HttpGet("activos")]
    public async Task<ActionResult<IEnumerable<AdministradorDto>>> GetActivos()
    {
        try
        {
            var administradores = await _administradorService.GetActivosAsync();
            return Ok(administradores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener administradores activos");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Activar/desactivar administrador
    /// </summary>
    [HttpPatch("{id}/estado")]
    public async Task<ActionResult<AdministradorDto>> CambiarEstado(Guid id, [FromBody] CambiarEstadoAdministradorDto estadoDto)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var administrador = await _administradorService.CambiarEstadoAsync(id, estadoDto.Activo);
            if (administrador == null)
                return NotFound("Administrador no encontrado");
                
            return Ok(administrador);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado del administrador {Id}", id);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// Buscar administradores por nombre
    /// </summary>
    [HttpGet("buscar")]
    public async Task<ActionResult<IEnumerable<AdministradorDto>>> BuscarPorNombre([FromQuery] string nombre)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(nombre) || nombre.Length < 2)
                return BadRequest("El nombre debe tener al menos 2 caracteres");

            var administradores = await _administradorService.BuscarPorNombreAsync(nombre);
            return Ok(administradores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al buscar administradores por nombre: {Nombre}", nombre);
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// DASHBOARD: Obtener estadísticas generales del sistema
    /// </summary>
    [HttpGet("dashboard/estadisticas")]
    public async Task<ActionResult<object>> GetEstadisticasGenerales()
    {
        try
        {
            var estadisticas = await _administradorService.GetEstadisticasGeneralesAsync();
            return Ok(estadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener estadísticas generales");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// DASHBOARD: Obtener estadísticas de reservas del día
    /// </summary>
    [HttpGet("dashboard/reservas-hoy")]
    public async Task<ActionResult<object>> GetReservasHoy()
    {
        try
        {
            var estadisticas = await _administradorService.GetReservasHoyAsync();
            return Ok(estadisticas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener reservas de hoy");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// DASHBOARD: Obtener alertas críticas del sistema
    /// </summary>
    [HttpGet("dashboard/alertas")]
    public async Task<ActionResult<object>> GetAlertasCriticas()
    {
        try
        {
            var alertas = await _administradorService.GetAlertasCriticasAsync();
            return Ok(alertas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener alertas críticas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// DASHBOARD: Obtener resumen de inventario con alertas de stock
    /// </summary>
    [HttpGet("dashboard/inventario-resumen")]
    public async Task<ActionResult<object>> GetResumenInventario()
    {
        try
        {
            var resumen = await _administradorService.GetResumenInventarioAsync();
            return Ok(resumen);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener resumen de inventario");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// DASHBOARD: Obtener empleados presentes hoy
    /// </summary>
    [HttpGet("dashboard/empleados-presentes")]
    public async Task<ActionResult<object>> GetEmpleadosPresentes()
    {
        try
        {
            var empleados = await _administradorService.GetEmpleadosPresentesHoyAsync();
            return Ok(empleados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener empleados presentes");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// REPORTES: Generar reporte de ventas por período
    /// </summary>
    [HttpGet("reportes/ventas")]
    public async Task<ActionResult<object>> GetReporteVentas(
        [FromQuery] DateTime fechaInicio, 
        [FromQuery] DateTime fechaFin)
    {
        try
        {
            if (fechaFin.Date < fechaInicio.Date)
                return BadRequest("La fecha de fin debe ser posterior o igual a la fecha de inicio");

            var reporte = await _administradorService.GetReporteVentasAsync(fechaInicio.Date, fechaFin.Date);
            return Ok(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de ventas");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// REPORTES: Generar reporte de ocupación por período
    /// </summary>
    [HttpGet("reportes/ocupacion")]
    public async Task<ActionResult<object>> GetReporteOcupacion(
        [FromQuery] DateTime fechaInicio, 
        [FromQuery] DateTime fechaFin)
    {
        try
        {
            if (fechaFin.Date < fechaInicio.Date)
                return BadRequest("La fecha de fin debe ser posterior o igual a la fecha de inicio");

            var reporte = await _administradorService.GetReporteOcupacionAsync(fechaInicio.Date, fechaFin.Date);
            return Ok(reporte);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar reporte de ocupación");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// GESTIÓN: Configurar parámetros del restaurante
    /// </summary>
    [HttpPost("configuracion")]
    public async Task<ActionResult> ConfigurarParametros([FromBody] ConfiguracionRestauranteDto configuracion)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _administradorService.ConfigurarParametrosAsync(configuracion);
            return Ok("Configuración actualizada exitosamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al configurar parámetros");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// GESTIÓN: Obtener configuración actual del restaurante
    /// </summary>
    [HttpGet("configuracion")]
    public async Task<ActionResult<object>> GetConfiguracion()
    {
        try
        {
            var configuracion = await _administradorService.GetConfiguracionAsync();
            return Ok(configuracion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener configuración");
            return StatusCode(500, "Error interno del servidor");
        }
    }

    /// <summary>
    /// AUDITORIA: Obtener log de actividades del sistema
    /// </summary>
    [HttpGet("auditoria/actividades")]
    public async Task<ActionResult<IEnumerable<object>>> GetLogActividades(
        [FromQuery] DateTime? fechaInicio = null,
        [FromQuery] DateTime? fechaFin = null,
        [FromQuery] string? usuario = null,
        [FromQuery] int limite = 100)
    {
        try
        {
            var logs = await _administradorService.GetLogActividadesAsync(
                fechaInicio ?? DateTime.Today.AddDays(-7),
                fechaFin ?? DateTime.Today.AddDays(1),
                usuario,
                limite);
                
            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener log de actividades");
            return StatusCode(500, "Error interno del servidor");
        }
    }
}