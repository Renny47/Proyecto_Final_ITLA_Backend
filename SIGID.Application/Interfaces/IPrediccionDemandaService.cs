using SIGID.Application.DTOs;

namespace SIGID.Application.Interfaces;

public interface IPrediccionDemandaService
{
    Task<PrediccionDemandaDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<PrediccionDemandaDto>> GetAllAsync();
    Task<PrediccionDemandaDto?> GetPrediccionActualAsync();
    Task<PrediccionDemandaDto?> GetUltimaPrediccionAsync();
    Task<PrediccionDemandaDto> CreatePrediccionAsync(CreatePrediccionDemandaDto createPrediccionDto);
    Task<PrediccionResultDto> CalcularPrediccionAsync(DateTime periodoInicio, DateTime periodoFin, bool consideraFestivos = true, bool consideraTendencias = true);
    Task<bool> DeleteAsync(Guid id);
    
    // Métodos específicos de predicción
    Task<decimal> GetPromedioReservasDiariasAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<decimal> GetPromedioPersonasPorReservaAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<decimal> GetPromedioIngresoPorPersonaAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<List<string>> GetElementosMasDemandadosAsync(DateTime fechaInicio, DateTime fechaFin);
}