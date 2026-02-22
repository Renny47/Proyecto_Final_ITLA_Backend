using Microsoft.Extensions.DependencyInjection;

namespace SIGID.Shared.Extensions;

public static class DependencyInjection
{
    /// <summary>
    /// Configuración base para todos los servicios compartidos
    /// </summary>
    public static IServiceCollection AddSharedServices(this IServiceCollection services)
    {
        // Aquí se pueden agregar servicios compartidos en el futuro
        return services;
    }
}