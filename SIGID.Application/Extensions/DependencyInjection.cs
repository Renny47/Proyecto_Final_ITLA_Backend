using Microsoft.Extensions.DependencyInjection;
using SIGID.Application.Interfaces;
using SIGID.Application.Security;
using SIGID.Application.Services;

namespace SIGID.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Servicios de aplicación
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        
        // Servicios de seguridad
        services.AddScoped<IJwtGenerator, JwtGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        
        return services;
    }
}