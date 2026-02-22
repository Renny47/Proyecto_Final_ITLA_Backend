using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SIGID.Application.Extensions;
using SIGID.Application.Interfaces;
using SIGID.Application.Services;
using SIGID.Domain.Entities;
using SIGID.Domain.Interfaces;
using SIGID.Infrastructure.Data;
using SIGID.Infrastructure.Repositories;
using SIGID.Shared.Configuration;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure port for Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection") ??
        Environment.GetEnvironmentVariable("DATABASE_URL") ??
        "Server=.\\SQLEXPRESS;Database=SIGID_DB;Trusted_Connection=true;TrustServerCertificate=true;",
        b => b.MigrationsAssembly("SIGID.API")));

// Configure Identity
builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireDigit = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();

// Configure JWT
var jwtSettings = new JwtSettings
{
    SecretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY") ?? builder.Configuration["JwtSettings:SecretKey"] ?? "mi-clave-secreta-super-ultra-mega-segura-para-jwt-token-que-debe-tener-al-menos-32-caracteres",
    Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["JwtSettings:Issuer"] ?? "SIGID-API",
    Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["JwtSettings:Audience"] ?? "SIGID-Client",
    TokenExpirationHours = int.Parse(Environment.GetEnvironmentVariable("JWT_EXPIRATION_HOURS") ?? builder.Configuration["JwtSettings:ExpirationInMinutes"] ?? "60") / 60
};
builder.Services.Configure<JwtSettings>(options =>
{
    options.SecretKey = jwtSettings.SecretKey;
    options.Issuer = jwtSettings.Issuer;
    options.Audience = jwtSettings.Audience;
    options.TokenExpirationHours = jwtSettings.TokenExpirationHours;
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

// Register application services
builder.Services.AddApplicationServices();
builder.Services.AddScoped<IBookingService, BookingService>();

// Infrastructure services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookingRepositoryAsync, BookingRepository>();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "🚀 SIGID Backend API", 
        Version = "v1",
        Description = "API Backend para sistema de autenticación con Clean Architecture",
        Contact = new OpenApiContact
        {
            Name = "SIGID Team",
            Email = "admin@sigid.com"
        }
    });

    // Configure JWT Authentication in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando el esquema Bearer. Ejemplo: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    // Enable XML comments
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Development", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
    
    options.AddPolicy("Production", policy =>
    {
        var allowedOrigins = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS")?.Split(',') ?? new[] { "https://*.railway.app", "https://*.up.railway.app" };
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Apply migrations ALWAYS (both Development and Production)
using (var scope = app.Services.CreateScope())
{
    try
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        logger.LogInformation("🔧 Applying database migrations...");
        logger.LogInformation($"Environment: {app.Environment.EnvironmentName}");
        logger.LogInformation($"Connection String: {builder.Configuration.GetConnectionString("DefaultConnection")?.Substring(0, 50)}...");
        
        await context.Database.MigrateAsync();
        logger.LogInformation("✅ Database migrations applied successfully!");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Error occurred while migrating the database");
        // Don't throw - let app start anyway
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIGID Backend API v1");
        c.RoutePrefix = string.Empty; // Swagger en la raíz
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
        c.DefaultModelsExpandDepth(-1);
    });
    app.UseCors("Development");
}
else
{
    // Production: también habilitar Swagger para Railway
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIGID Backend API v1");
        c.RoutePrefix = "swagger"; // Swagger en /swagger en producción
    });
    app.UseCors("Production");
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Welcome endpoint
app.MapGet("/api", () => new
{
    message = "🚀 Bienvenido a SIGID Backend API!",
    timestamp = DateTime.UtcNow,
    swagger = "/swagger",
    endpoints = new[]
    {
        "POST /api/auth/login - Iniciar sesión",
        "POST /api/auth/register - Registrar usuario",
        "GET /api/auth/profile - Obtener perfil (requiere token)",
        "GET /api/auth/status - Estado del servicio"
    }
}).WithName("Welcome");

app.Run();
