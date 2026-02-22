using System.ComponentModel.DataAnnotations;

namespace SIGID.Application.DTOs;

public class PrediccionDemandaDto
{
    public Guid Id { get; set; }
    public DateTime FechaPrediccion { get; set; }
    public DateTime PeriodoInicio { get; set; }
    public DateTime PeriodoFin { get; set; }
    public int ReservasPronosticadas { get; set; }
    public int PersonasEstimadas { get; set; }
    public decimal IngresosEstimados { get; set; }
    public string ElementosAltos { get; set; } = string.Empty;
    public string ElementosMedios { get; set; } = string.Empty;
    public string ElementosBajos { get; set; } = string.Empty;
    public decimal PromedioReservasDiarias { get; set; }
    public decimal PromedioPersonasPorReserva { get; set; }
    public decimal PromedioIngresoPorPersona { get; set; }
    public bool ConsideraFestivos { get; set; }
    public bool ConsideraTendencias { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool EsPrediccionActual { get; set; }
}

public class CreatePrediccionDemandaDto
{
    [Required(ErrorMessage = "El periodo de inicio es requerido")]
    [DataType(DataType.Date, ErrorMessage = "Formato de fecha inválido")]
    public DateTime PeriodoInicio { get; set; }
    
    [Required(ErrorMessage = "El periodo de fin es requerido")]
    [DataType(DataType.Date, ErrorMessage = "Formato de fecha inválido")]
    public DateTime PeriodoFin { get; set; }
    
    public bool ConsideraFestivos { get; set; } = true;
    public bool ConsideraTendencias { get; set; } = true;
}

public class PrediccionResultDto
{
    public int ReservasPronosticadas { get; set; }
    public int PersonasEstimadas { get; set; }
    public decimal IngresosEstimados { get; set; }
    public List<string> ElementosRecomendados { get; set; } = new();
    public List<string> AlertasStock { get; set; } = new();
    public string PeriodoAnalisis { get; set; } = string.Empty;
    public DateTime FechaCalculo { get; set; }
}

public class StockAlertDto
{
    public Guid InventarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CantidadActual { get; set; }
    public int CantidadMinima { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string TipoAlerta { get; set; } = string.Empty; // "Stock Bajo", "Vencido", "Próximo a Vencer"
    public string Mensaje { get; set; } = string.Empty;
    public DateTime FechaAlerta { get; set; }
    public string Prioridad { get; set; } = string.Empty; // "Alta", "Media", "Baja"
}