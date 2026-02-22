using SIGID.Application.DTOs;

namespace SIGID.Application.Interfaces;

public interface IAdministradorService
{
    Task<IEnumerable<AdministradorDto>> GetAllAsync();
    Task<AdministradorDto?> GetByIdAsync(Guid id);
    Task<AdministradorDto> CreateAsync(CreateAdministradorDto createAdministradorDto);
    Task<AdministradorDto?> UpdateAsync(Guid id, UpdateAdministradorDto updateAdministradorDto);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<AdministradorDto>> GetActivosAsync();
    Task<AdministradorDto?> CambiarEstadoAsync(Guid id, bool activo);
    Task<IEnumerable<AdministradorDto>> BuscarPorNombreAsync(string nombre);
    
    // Dashboard y reportes
    Task<object> GetEstadisticasGeneralesAsync();
    Task<object> GetReservasHoyAsync();
    Task<object> GetAlertasCriticasAsync();
    Task<object> GetResumenInventarioAsync();
    Task<object> GetEmpleadosPresentesHoyAsync();
    
    // Reportes avanzados
    Task<object> GetReporteVentasAsync(DateTime fechaInicio, DateTime fechaFin);
    Task<object> GetReporteOcupacionAsync(DateTime fechaInicio, DateTime fechaFin);
    
    // Configuración
    Task ConfigurarParametrosAsync(ConfiguracionRestauranteDto configuracion);
    Task<object> GetConfiguracionAsync();
    
    // Auditoría
    Task<IEnumerable<object>> GetLogActividadesAsync(DateTime fechaInicio, DateTime fechaFin, string? usuario, int limite);
}