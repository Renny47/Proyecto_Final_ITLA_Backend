using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SIGID.Application.DTOs;
using SIGID.Application.Interfaces;
using SIGID.Domain.Entities;
using SIGID.Domain.Interfaces;

namespace SIGID.Application.Services;

public class PrediccionDemandaService : IPrediccionDemandaService
{
    private readonly IPrediccionDemandaRepository _prediccionRepository;
    private readonly IReservaRepository _reservaRepository;
    private readonly IInventarioRepository _inventarioRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<PrediccionDemandaService> _logger;

    public PrediccionDemandaService(
        IPrediccionDemandaRepository prediccionRepository,
        IReservaRepository reservaRepository,
        IInventarioRepository inventarioRepository,
        IMapper mapper,
        ILogger<PrediccionDemandaService> logger)
    {
        _prediccionRepository = prediccionRepository;
        _reservaRepository = reservaRepository;
        _inventarioRepository = inventarioRepository;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PrediccionDemandaDto?> GetByIdAsync(Guid id)
    {
        var prediccion = await _prediccionRepository.GetByIdAsync(id);
        return prediccion != null ? _mapper.Map<PrediccionDemandaDto>(prediccion) : null;
    }

    public async Task<IEnumerable<PrediccionDemandaDto>> GetAllAsync()
    {
        var predicciones = await _prediccionRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<PrediccionDemandaDto>>(predicciones);
    }

    public async Task<PrediccionDemandaDto?> GetPrediccionActualAsync()
    {
        var prediccion = await _prediccionRepository.GetPrediccionActualAsync();
        return prediccion != null ? _mapper.Map<PrediccionDemandaDto>(prediccion) : null;
    }

    public async Task<PrediccionDemandaDto?> GetUltimaPrediccionAsync()
    {
        var prediccion = await _prediccionRepository.GetUltimaPrediccionAsync();
        return prediccion != null ? _mapper.Map<PrediccionDemandaDto>(prediccion) : null;
    }

    public async Task<PrediccionDemandaDto> CreatePrediccionAsync(CreatePrediccionDemandaDto createPrediccionDto)
    {
        var resultado = await CalcularPrediccionAsync(
            createPrediccionDto.PeriodoInicio,
            createPrediccionDto.PeriodoFin,
            createPrediccionDto.ConsideraFestivos,
            createPrediccionDto.ConsideraTendencias);

        var prediccion = new PrediccionDemanda
        {
            Id = Guid.NewGuid(),
            FechaPrediccion = DateTime.UtcNow,
            PeriodoInicio = createPrediccionDto.PeriodoInicio,
            PeriodoFin = createPrediccionDto.PeriodoFin,
            ReservasPronosticadas = resultado.ReservasPronosticadas,
            PersonasEstimadas = resultado.PersonasEstimadas,
            IngresosEstimados = resultado.IngresosEstimados,
            ElementosAltos = JsonConvert.SerializeObject(resultado.ElementosRecomendados.Take(5)),
            ElementosMedios = JsonConvert.SerializeObject(resultado.ElementosRecomendados.Skip(5).Take(10)),
            ElementosBajos = JsonConvert.SerializeObject(resultado.ElementosRecomendados.Skip(15)),
            ConsideraFestivos = createPrediccionDto.ConsideraFestivos,
            ConsideraTendencias = createPrediccionDto.ConsideraTendencias,
            CreatedAt = DateTime.UtcNow
        };

        // Calcular promedios históricos para la predicción
        var fechaHistoricaInicio = createPrediccionDto.PeriodoInicio.AddDays(-30);
        var fechaHistoricaFin = createPrediccionDto.PeriodoInicio.AddDays(-1);

        prediccion.PromedioReservasDiarias = await GetPromedioReservasDiariasAsync(fechaHistoricaInicio, fechaHistoricaFin);
        prediccion.PromedioPersonasPorReserva = await GetPromedioPersonasPorReservaAsync(fechaHistoricaInicio, fechaHistoricaFin);
        prediccion.PromedioIngresoPorPersona = await GetPromedioIngresoPorPersonaAsync(fechaHistoricaInicio, fechaHistoricaFin);

        var nuevaPrediccion = await _prediccionRepository.CreateAsync(prediccion);
        
        _logger.LogInformation("Nueva predicción creada: {PeriodoInicio} - {PeriodoFin}, Reservas estimadas: {Reservas}",
            prediccion.PeriodoInicio, prediccion.PeriodoFin, prediccion.ReservasPronosticadas);

        return _mapper.Map<PrediccionDemandaDto>(nuevaPrediccion);
    }

    public async Task<PrediccionResultDto> CalcularPrediccionAsync(DateTime periodoInicio, DateTime periodoFin, 
        bool consideraFestivos = true, bool consideraTendencias = true)
    {
        // Período histórico para análisis (30 días anteriores)
        var fechaHistoricaInicio = periodoInicio.AddDays(-30);
        var fechaHistoricaFin = periodoInicio.AddDays(-1);

        // Obtener datos históricos
        var reservasHistoricas = await _reservaRepository.GetByFechaRangoAsync(fechaHistoricaInicio, fechaHistoricaFin);
        
        if (!reservasHistoricas.Any())
        {
            _logger.LogWarning("No hay datos históricos suficientes para realizar predicción");
            return new PrediccionResultDto
            {
                ReservasPronosticadas = 0,
                PersonasEstimadas = 0,
                IngresosEstimados = 0,
                ElementosRecomendados = new List<string>(),
                AlertasStock = new List<string>(),
                PeriodoAnalisis = $"{periodoInicio:dd/MM/yyyy} - {periodoFin:dd/MM/yyyy}",
                FechaCalculo = DateTime.UtcNow
            };
        }

        // Cálculos básicos
        var diasAnalisis = (periodoFin - periodoInicio).Days + 1;
        var diasHistoricos = (fechaHistoricaFin - fechaHistoricaInicio).Days + 1;
        
        var promedioReservasDiarias = (decimal)reservasHistoricas.Count() / diasHistoricos;
        var promedioPersonasPorReserva = reservasHistoricas.Any() ? 
            (decimal)reservasHistoricas.Sum(r => r.NumeroPersonas) / reservasHistoricas.Count() : 0;
        var promedioIngresoPorPersona = reservasHistoricas.Any() && reservasHistoricas.Any(r => r.MontoEstimado.HasValue) ?
            reservasHistoricas.Where(r => r.MontoEstimado.HasValue).Average(r => r.MontoEstimado!.Value) / promedioPersonasPorReserva : 50; // Default 50 por persona

        // Aplicar factores de ajuste
        var factorTendencia = consideraTendencias ? CalcularFactorTendencia(reservasHistoricas) : 1.0m;
        var factorFestivos = consideraFestivos ? CalcularFactorFestivos(periodoInicio, periodoFin) : 1.0m;

        // Predicciones finales
        var reservasPronosticadas = (int)(promedioReservasDiarias * diasAnalisis * factorTendencia * factorFestivos);
        var personasEstimadas = (int)(reservasPronosticadas * promedioPersonasPorReserva);
        var ingresosEstimados = personasEstimadas * promedioIngresoPorPersona;

        // Elementos recomendados y alertas
        var elementosRecomendados = await GetElementosMasDemandadosAsync(fechaHistoricaInicio, fechaHistoricaFin);
        var alertasStock = await GenerarAlertasStockAsync(personasEstimadas);

        return new PrediccionResultDto
        {
            ReservasPronosticadas = reservasPronosticadas,
            PersonasEstimadas = personasEstimadas,
            IngresosEstimados = ingresosEstimados,
            ElementosRecomendados = elementosRecomendados,
            AlertasStock = alertasStock,
            PeriodoAnalisis = $"{periodoInicio:dd/MM/yyyy} - {periodoFin:dd/MM/yyyy}",
            FechaCalculo = DateTime.UtcNow
        };
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _prediccionRepository.DeleteAsync(id);
    }

    public async Task<decimal> GetPromedioReservasDiariasAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var reservas = await _reservaRepository.GetByFechaRangoAsync(fechaInicio, fechaFin);
        var dias = (fechaFin - fechaInicio).Days + 1;
        return dias > 0 ? (decimal)reservas.Count() / dias : 0;
    }

    public async Task<decimal> GetPromedioPersonasPorReservaAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var reservas = await _reservaRepository.GetByFechaRangoAsync(fechaInicio, fechaFin);
        return reservas.Any() ? (decimal)reservas.Sum(r => r.NumeroPersonas) / reservas.Count() : 0;
    }

    public async Task<decimal> GetPromedioIngresoPorPersonaAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        var reservas = await _reservaRepository.GetByFechaRangoAsync(fechaInicio, fechaFin);
        var reservasConMonto = reservas.Where(r => r.MontoEstimado.HasValue && r.MontoEstimado.Value > 0);
        
        if (!reservasConMonto.Any()) return 50; // Valor por defecto

        var totalIngresos = reservasConMonto.Sum(r => r.MontoEstimado!.Value);
        var totalPersonas = reservasConMonto.Sum(r => r.NumeroPersonas);
        
        return totalPersonas > 0 ? totalIngresos / totalPersonas : 50;
    }

    public async Task<List<string>> GetElementosMasDemandadosAsync(DateTime fechaInicio, DateTime fechaFin)
    {
        // Simulación de lógica de elementos más demandados
        // En un caso real, esto estaría basado en órdenes históricas
        await Task.CompletedTask;
        
        var elementos = new List<string>
        {
            "Agua Mineral - Bebidas",
            "Pollo a la Plancha - Comidas", 
            "Ensalada César - Comidas",
            "Cerveza Local - Bebidas",
            "Pasta Alfredo - Comidas",
            "Jugo Natural - Bebidas",
            "Arroz con Pollo - Comidas",
            "Soda - Bebidas",
            "Pescado Frito - Comidas",
            "Café - Bebidas"
        };

        return elementos;
    }

    private decimal CalcularFactorTendencia(IEnumerable<Reserva> reservasHistoricas)
    {
        // Lógica simple de tendencia: comparar primera y segunda mitad del período
        var reservasList = reservasHistoricas.OrderBy(r => r.FechaReserva).ToList();
        if (reservasList.Count < 4) return 1.0m;

        var mitad = reservasList.Count / 2;
        var primeraMitad = reservasList.Take(mitad).Count();
        var segundaMitad = reservasList.Skip(mitad).Count();

        if (primeraMitad == 0) return 1.0m;

        var crecimiento = (decimal)segundaMitad / primeraMitad;
        
        // Limitar el factor de tendencia entre 0.8 y 1.5
        return Math.Min(Math.Max(crecimiento, 0.8m), 1.5m);
    }

    private decimal CalcularFactorFestivos(DateTime periodoInicio, DateTime periodoFin)
    {
        // Factor simple basado en días de fin de semana en el período
        var factorBase = 1.0m;
        var totalDias = (periodoFin - periodoInicio).Days + 1;
        var diasFinDeSemana = 0;

        for (var fecha = periodoInicio; fecha <= periodoFin; fecha = fecha.AddDays(1))
        {
            if (fecha.DayOfWeek == DayOfWeek.Saturday || fecha.DayOfWeek == DayOfWeek.Sunday)
                diasFinDeSemana++;
        }

        var porcentajeFinDeSemana = (decimal)diasFinDeSemana / totalDias;
        
        // Mayor demanda en fin de semana
        return factorBase + (porcentajeFinDeSemana * 0.3m); // Hasta 30% más demanda
    }

    private async Task<List<string>> GenerarAlertasStockAsync(int personasEstimadas)
    {
        var alertas = new List<string>();
        var inventarios = await _inventarioRepository.GetAllAsync();

        foreach (var item in inventarios)
        {
            // Estimación simple: cada persona consume en promedio cierta cantidad
            var consumoEstimado = item.Categoria switch
            {
                Domain.Enums.CategoriaInventario.Bebidas => (int)(personasEstimadas * 0.8), // 80% toman bebida
                Domain.Enums.CategoriaInventario.Comidas => (int)(personasEstimadas * 0.6), // 60% comen plato principal
                Domain.Enums.CategoriaInventario.Ingredientes => (int)(personasEstimadas * 0.4), // Para cocinar
                _ => (int)(personasEstimadas * 0.2)
            };

            if (item.CantidadActual < consumoEstimado)
            {
                var deficit = consumoEstimado - item.CantidadActual;
                alertas.Add($"⚠️ {item.Nombre}: Stock insuficiente. Necesita {deficit} {item.Unidad} adicionales");
            }
        }

        return alertas;
    }
}