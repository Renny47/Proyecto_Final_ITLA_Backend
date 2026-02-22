namespace SIGID.Domain.Entities;

public class PrediccionDemanda
{
    public Guid Id { get; set; }
    public DateTime FechaPrediccion { get; set; }
    public DateTime PeriodoInicio { get; set; }
    public DateTime PeriodoFin { get; set; }
    
    // Métricas de Predicción
    public int ReservasPronosticadas { get; set; }
    public int PersonasEstimadas { get; set; }
    public decimal IngresosEstimados { get; set; }
    
    // Elementos de Inventario más demandados
    public string ElementosAltos { get; set; } = string.Empty; // JSON de items
    public string ElementosMedios { get; set; } = string.Empty; // JSON de items
    public string ElementosBajos { get; set; } = string.Empty; // JSON de items
    
    // Métricas de cálculo
    public decimal PromedioReservasDiarias { get; set; }
    public decimal PromedioPersonasPorReserva { get; set; }
    public decimal PromedioIngresoPorPersona { get; set; }
    
    // Factores considerados
    public bool ConsideraFestivos { get; set; }
    public bool ConsideraTendencias { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Calculated Property
    public bool EsPrediccionActual => PeriodoInicio <= DateTime.Now && PeriodoFin >= DateTime.Now;
}