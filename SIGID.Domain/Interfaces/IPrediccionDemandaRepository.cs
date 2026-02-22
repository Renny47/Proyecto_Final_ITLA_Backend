using SIGID.Domain.Entities;

namespace SIGID.Domain.Interfaces;

public interface IPrediccionDemandaRepository
{
    Task<PrediccionDemanda?> GetByIdAsync(Guid id);
    Task<IEnumerable<PrediccionDemanda>> GetAllAsync();
    Task<PrediccionDemanda?> GetPrediccionActualAsync();
    Task<IEnumerable<PrediccionDemanda>> GetPrediccionesPorRangoAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<PrediccionDemanda?> GetUltimaPrediccionAsync();
    Task<PrediccionDemanda> CreateAsync(PrediccionDemanda prediccion);
    Task<PrediccionDemanda> UpdateAsync(PrediccionDemanda prediccion);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
}