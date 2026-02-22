using AutoMapper;
using Microsoft.Extensions.Logging;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;
using SIGID.Domain.Entities;
using SIGID.Domain.Interfaces;

namespace SIGID.Application.Services;

public class AdministradorService : IAdministradorService
{
    private readonly IAdministradorRepository _administradorRepository;
    private readonly IReservaRepository _reservaRepository;
    private readonly IInventarioRepository _inventarioRepository;
    private readonly IEmpleadoRepository _empleadoRepository;
    private readonly ITurnoRepository _turnoRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AdministradorService> _logger;

    public AdministradorService(
        IAdministradorRepository administradorRepository,
        IReservaRepository reservaRepository,
        IInventarioRepository inventarioRepository,
        IEmpleadoRepository empleadoRepository,
        ITurnoRepository turnoRepository,
        IMapper mapper,
        ILogger<AdministradorService> logger)
    {
        _administradorRepository = administradorRepository;
        _reservaRepository = reservaRepository;
        _inventarioRepository = inventarioRepository;
        _empleadoRepository = empleadoRepository;
        _turnoRepository = turnoRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IEnumerable<AdministradorDto>> GetAllAsync()
    {
        var administradores = await _administradorRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<AdministradorDto>>(administradores);
    }

    public async Task<AdministradorDto?> GetByIdAsync(Guid id)
    {
        var administrador = await _administradorRepository.GetByIdAsync(id);
        return administrador != null ? _mapper.Map<AdministradorDto>(administrador) : null;
    }

    public async Task<AdministradorDto> CreateAsync(CreateAdministradorDto createAdministradorDto)
    {
        var administrador = _mapper.Map<Administrador>(createAdministradorDto);
        administrador.Id = Guid.NewGuid();
        administrador.FechaAsignacion = DateTime.UtcNow;

        var createdAdministrador = await _administradorRepository.CreateAsync(administrador);
        _logger.LogInformation("Administrador creado: {Id}", createdAdministrador.Id);
        
        return _mapper.Map<AdministradorDto>(createdAdministrador);
    }

    public async Task<AdministradorDto?> UpdateAsync(Guid id, UpdateAdministradorDto updateAdministradorDto)
    {
        var administrador = await _administradorRepository.GetByIdAsync(id);
        if (administrador == null) return null;

        _mapper.Map(updateAdministradorDto, administrador);
        
        var updatedAdministrador = await _administradorRepository.UpdateAsync(administrador);
        _logger.LogInformation("Administrador actualizado: {Id}", id);
        
        return _mapper.Map<AdministradorDto>(updatedAdministrador);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var result = await _administradorRepository.DeleteAsync(id);
        if (result) _logger.LogInformation("Administrador eliminado: {Id}", id);
        return result;
    }

    public async Task<IEnumerable<AdministradorDto>> GetActivosAsync()
    {
        var administradores = await _administradorRepository.GetActivosAsync();
        return _mapper.Map<IEnumerable<AdministradorDto>>(administradores);
    }

    public async Task<AdministradorDto?> CambiarEstadoAsync(Guid id, bool activo)
    {
        var administrador = await _administradorRepository.GetByIdAsync(id);
        if (administrador == null) return null;

        administrador.Activo = activo;
        var updatedAdministrador = await _administradorRepository.UpdateAsync(administrador);
        
        _logger.LogInformation("Estado de administrador cambiado: {Id} -> {Activo}", id, activo);
        return _mapper.Map<AdministradorDto>(updatedAdministrador);
    }

    public async Task<IEnumerable<AdministradorDto>> BuscarPorNombreAsync(string nombre)
    {
        var administradores = await _administradorRepository.GetAllAsync();
        var filtrados = administradores.Where(a => 
            a.Usuario.FirstName.Contains(nombre, StringComparison.OrdinalIgnoreCase) ||
            a.Usuario.LastName.Contains(nombre, StringComparison.OrdinalIgnoreCase) ||
            a.Usuario.Email.Contains(nombre, StringComparison.OrdinalIgnoreCase));
        return _mapper.Map<IEnumerable<AdministradorDto>>(filtrados);
    }

    public async Task<object> GetEstadisticasGeneralesAsync()
    {
        var hoy = DateTime.Today;
        
        var reservas = await _reservaRepository.GetAllAsync();
        var reservasHoy = reservas.Where(r => r.FechaReserva.Date == hoy);
        var empleados = await _empleadoRepository.GetAllAsync();
        var empleadosActivos = empleados.Where(e => e.Activo);
        var inventario = await _inventarioRepository.GetAllAsync();
        var inventarioBajo = inventario.Where(i => i.RequiereReposicion);

        return new
        {
            totalReservas = reservas.Count(),
            reservasHoy = reservasHoy.Count(),
            totalEmpleados = empleadosActivos.Count(),
            productosConBajoStock = inventarioBajo.Count(),
            fecha = hoy
        };
    }

    public async Task<object> GetReservasHoyAsync()
    {
        var hoy = DateTime.Today;
        var reservas = await _reservaRepository.GetAllAsync();
        var reservasHoy = reservas.Where(r => r.FechaReserva.Date == hoy);
        
        return new
        {
            fecha = hoy,
            totalReservas = reservasHoy.Count(),
            personasTotal = reservasHoy.Sum(r => r.NumeroPersonas),
            ingresoEstimado = reservasHoy.Sum(r => r.MontoEstimado ?? 0),
            reservasPorEstado = reservasHoy.GroupBy(r => r.Estado).Select(g => new
            {
                estado = g.Key.ToString(),
                cantidad = g.Count()
            })
        };
    }

    public async Task<object> GetAlertasCriticasAsync()
    {
        var alertas = new List<object>();
        
        // Alertas de inventario
        var inventario = await _inventarioRepository.GetAllAsync();
        var productosBajoStock = inventario.Where(i => i.RequiereReposicion);
        foreach (var producto in productosBajoStock)
        {
            alertas.Add(new
            {
                tipo = "Inventario",
                severidad = "Alta",
                mensaje = $"Stock crítico: {producto.Nombre} ({producto.CantidadActual}/{producto.CantidadMinima})",
                fecha = DateTime.UtcNow
            });
        }

        // Alertas básicas de empleados
        var empleados = await _empleadoRepository.GetAllAsync();
        var empleadosInactivos = empleados.Where(e => !e.Activo);
        if (empleadosInactivos.Any())
        {
            alertas.Add(new
            {
                tipo = "Personal",
                severidad = "Media",
                mensaje = $"{empleadosInactivos.Count()} empleados inactivos en el sistema",
                fecha = DateTime.UtcNow
            });
        }

        return alertas;
    }

    public async Task<object> GetResumenInventarioAsync()
    {
        var productos = await _inventarioRepository.GetAllAsync();
        
        return new
        {
            totalProductos = productos.Count(),
            stockCritico = productos.Count(p => p.RequiereReposicion),
            productosVencidos = productos.Count(p => p.EstaVencido),
            valorTotalInventario = productos.Sum(p => p.CantidadActual * p.PrecioCosto),
            categorias = productos.GroupBy(p => p.Categoria).Select(g => new
            {
                categoria = g.Key.ToString(),
                productos = g.Count(),
                valor = g.Sum(p => p.CantidadActual * p.PrecioCosto)
            })
        };
    }

    public async Task<object> GetEmpleadosPresentesHoyAsync()
    {
        var hoy = DateTime.Today;
        var turnos = await _turnoRepository.GetAllAsync();
        var turnosHoy = turnos.Where(t => t.FechaTurno.Date == hoy);
        
        return new
        {
            fecha = hoy,
            empleadosPresentes = turnosHoy.Count(),
            turnosPorDepartamento = turnosHoy.GroupBy(t => t.Empleado.Departamento).Select(g => new
            {
                departamento = g.Key ?? "Sin asignar",
                empleados = g.Count()
            })
        };
    }

    public async Task<object> GetReporteVentasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var reservas = await _reservaRepository.GetAllAsync();
        var reservasEnRango = reservas.Where(r => r.FechaReserva.Date >= fechaInicio.Date && r.FechaReserva.Date <= fechaFin.Date);
        var reservasConfirmadas = reservasEnRango.Where(r => r.Estado == Domain.Enums.EstadoReserva.Confirmada);
        
        return new
        {
            periodo = new { fechaInicio, fechaFin },
            totalVentas = reservasConfirmadas.Sum(r => r.MontoEstimado ?? 0),
            totalReservas = reservasConfirmadas.Count(),
            promedioVentaDiaria = reservasConfirmadas.Sum(r => r.MontoEstimado ?? 0) / Math.Max(1, (fechaFin - fechaInicio).Days),
            ventasPorDia = reservasConfirmadas.GroupBy(r => r.FechaReserva.Date).Select(g => new
            {
                fecha = g.Key,
                ventas = g.Sum(r => r.MontoEstimado ?? 0),
                reservas = g.Count()
            })
        };
    }

    public async Task<object> GetReporteOcupacionAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var reservas = await _reservaRepository.GetAllAsync();
        var reservasEnRango = reservas.Where(r => r.FechaReserva.Date >= fechaInicio.Date && r.FechaReserva.Date <= fechaFin.Date);
        
        return new
        {
            periodo = new { fechaInicio, fechaFin },
            ocupacionPromedio = reservasEnRango.Any() ? reservasEnRango.Average(r => r.NumeroPersonas) : 0,
            personasTotales = reservasEnRango.Sum(r => r.NumeroPersonas),
            ocupacionPorDia = reservasEnRango.GroupBy(r => r.FechaReserva.Date).Select(g => new
            {
                fecha = g.Key,
                personas = g.Sum(r => r.NumeroPersonas),
                reservas = g.Count()
            })
        };
    }

    public async Task ConfigurarParametrosAsync(ConfiguracionRestauranteDto configuracion)
    {
        // En una implementación real, esto se guardaría en una tabla de configuración
        _logger.LogInformation("Configuración de restaurante actualizada");
        await Task.CompletedTask;
    }

    public async Task<object> GetConfiguracionAsync()
    {
        // En una implementación real, esto se leería de una tabla de configuración
        return new
        {
            capacidadMaxima = 100,
            horaApertura = 8,
            horaCierre = 22,
            tiempoPromedioMesa = 90,
            margenGanancia = 25.0m
        };
    }

    public async Task<IEnumerable<object>> GetLogActividadesAsync(DateTime fechaInicio, DateTime fechaFin, string? usuario, int limite)
    {
        // En una implementación real, esto consultaría una tabla de logs/auditoría
        var logs = new List<object>
        {
            new { fecha = DateTime.Now, usuario = "admin", actividad = "Login al sistema", ip = "192.168.1.1" },
            new { fecha = DateTime.Now.AddMinutes(-30), usuario = "admin", actividad = "Creación de reserva", ip = "192.168.1.1" }
        };
        
        return await Task.FromResult(logs.Take(limite));
    }
}