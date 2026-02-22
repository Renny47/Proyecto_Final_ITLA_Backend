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
using System.Threading;

var builder = WebApplication.CreateBuilder(args);

// Configure port for Railway
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// Add services to the container.
builder.Services.AddControllers();

// Configure Entity Framework
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") ?? 
                       builder.Configuration.GetConnectionString("DefaultConnection");

// If using Railway PostgreSQL, convert the URL format
if (connectionString?.StartsWith("postgres://") == true)
{
    var uri = new Uri(connectionString);
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true";
}
// Default for development if no proper connection string
else if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("PASSWORD_PLACEHOLDER"))
{
    connectionString = "Server=.\\SQLEXPRESS;Database=SIGID_DB;Trusted_Connection=true;TrustServerCertificate=true;";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString, b => b.MigrationsAssembly("SIGID.API")));

// Configure Identity
builder.Services.AddIdentity<Usuario, IdentityRole>(options =>
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

// SmartResto Application Services
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<IInventarioService, InventarioService>();
builder.Services.AddScoped<IPrediccionDemandaService, PrediccionDemandaService>();
builder.Services.AddScoped<IAdministradorService, AdministradorService>();

// Infrastructure services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUsuarioRepository, UserRepository>();

// SmartResto Infrastructure Repositories
builder.Services.AddScoped<IReservaRepository, ReservaRepository>();
builder.Services.AddScoped<IInventarioRepository, InventarioRepository>();
builder.Services.AddScoped<IPrediccionDemandaRepository, PrediccionDemandaRepository>();
builder.Services.AddScoped<IAdministradorRepository, AdministradorRepository>();
builder.Services.AddScoped<IEmpleadoRepository, EmpleadoRepository>();
builder.Services.AddScoped<ITurnoRepository, TurnoRepository>();

// AutoMapper configuration (base)
builder.Services.AddAutoMapper(typeof(SIGID.Application.Mappings.SmartRestoMappingProfile));

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

// Apply migrations ALWAYS (both Development and Production) with timeout
using (var scope = app.Services.CreateScope())
{
    try
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        
        logger.LogInformation("🔧 Attempting database migrations...");
        logger.LogInformation($"Environment: {app.Environment.EnvironmentName}");
        
        // Use a timeout for migration to prevent Railway healthcheck timeout
        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
        
        // Test connection first
        logger.LogInformation("🔍 Testing database connection...");
        var canConnect = await context.Database.CanConnectAsync(cancellationTokenSource.Token);
        if (canConnect)
        {
            logger.LogInformation("✅ Database connection successful");
            
            // Apply migrations
            await context.Database.MigrateAsync(cancellationTokenSource.Token);
            logger.LogInformation("✅ Database migrations applied successfully!");
        }
        else
        {
            logger.LogWarning("⚠️ Cannot connect to database - skipping migrations");
        }
    }
    catch (OperationCanceledException)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("⏱️ Database migration timed out - app will continue starting");
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "⚠️ Database migration failed - app will continue starting:");
        logger.LogWarning($"Migration error: {ex.Message}");
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

// No redirigir a HTTPS en producción detrás de proxy (Railway); evita 307 y fallo de healthcheck
if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Healthcheck mínimo (sin BD ni auth) para Railway - más robusto y rápido
app.MapGet("/health", () => 
{
    return Results.Ok(new 
    { 
        status = "healthy", 
        timestamp = DateTime.UtcNow,
        service = "SIGID-API",
        version = "1.0.0"
    });
});

// Database health check (separate endpoint for detailed checks)
app.MapGet("/health/database", async (AppDbContext context, ILogger<Program> logger) =>
{
    try
    {
        // Simple check to see if database is accessible
        var result = await context.Database.ExecuteSqlRawAsync("SELECT 1");
        return Results.Ok(new 
        { 
            status = "healthy", 
            database = "connected",
            timestamp = DateTime.UtcNow
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database health check failed");
        return Results.Problem(new 
        { 
            status = "unhealthy", 
            database = "disconnected",
            error = ex.Message,
            timestamp = DateTime.UtcNow
        }.ToString() ?? "Database connection failed");
    }
});

// Basic ping endpoint for load balancers
app.MapGet("/ping", () => "pong");

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
