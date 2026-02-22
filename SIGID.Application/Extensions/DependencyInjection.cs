using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using SIGID.Application.Interfaces;
using SIGID.Application.Security;
using SIGID.Application.Services;
using SIGID.Application.Validators;

namespace SIGID.Application.Extensions;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // Add application services
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IGoogleAuthService, GoogleAuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtGenerator, JwtGenerator>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();

        // Add FluentValidation validators
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();

        return services;
    }
}