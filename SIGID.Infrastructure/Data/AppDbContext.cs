using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIGID.Domain.Entities;

namespace SIGID.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<Usuario>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Entidades SmartResto (Users/Usuarios viene de IdentityDbContext<Usuario>)
    public DbSet<Empleado> Empleados { get; set; }
    public DbSet<Administrador> Administradores { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<Turno> Turnos { get; set; }
    public DbSet<Inventario> Inventarios { get; set; }
    public DbSet<PrediccionDemanda> PrediccionesDemanda { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuraciones para Usuario (entidad única de identidad)
        builder.Entity<Usuario>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Direccion).HasMaxLength(200);
            entity.Property(e => e.TipoUsuario).IsRequired();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.UserName).IsUnique();

            // Relación uno a muchos: Usuario -> Reservas
            entity.HasMany(u => u.Reservas)
                  .WithOne(r => r.Usuario)
                  .HasForeignKey(r => r.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Relación uno a uno: Usuario -> Empleado
            entity.HasOne(u => u.Empleado)
                  .WithOne(e => e.Usuario)
                  .HasForeignKey<Empleado>(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Relación uno a uno: Usuario -> Administrador
            entity.HasOne(u => u.Administrador)
                  .WithOne(a => a.Usuario)
                  .HasForeignKey<Administrador>(a => a.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuraciones para Empleado
        builder.Entity<Empleado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Puesto).HasMaxLength(50).IsRequired();
            entity.Property(e => e.SalarioHora).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.FechaContratacion).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Departamento).HasMaxLength(50);
            entity.Property(e => e.Supervisor).HasMaxLength(100);

            // Relación muchos a uno: Empleado -> Usuario
            entity.HasOne(e => e.Usuario)
                  .WithOne(u => u.Empleado)
                  .HasForeignKey<Empleado>(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuraciones para Administrador
        builder.Entity<Administrador>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NivelAcceso).HasMaxLength(20).IsRequired();
            entity.Property(e => e.FechaAsignacion).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.AreaResponsabilidad).HasMaxLength(100);

            // Relación muchos a uno: Administrador -> Usuario
            entity.HasOne(a => a.Usuario)
                  .WithOne(u => u.Administrador)
                  .HasForeignKey<Administrador>(a => a.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuraciones para Reserva
        builder.Entity<Reserva>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NumeroPersonas).IsRequired();
            entity.Property(e => e.FechaReserva).IsRequired();
            entity.Property(e => e.FechaHoraReserva).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.Estado).IsRequired();
            entity.Property(e => e.Comentarios).HasMaxLength(500);
            entity.Property(e => e.NumeroMesa).HasMaxLength(10);
            entity.Property(e => e.MontoEstimado).HasColumnType("decimal(18,2)");
            
            // Relación muchos a uno: Reservas -> Usuario
            entity.HasOne(r => r.Usuario)
                  .WithMany(u => u.Reservas)
                  .HasForeignKey(r => r.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Índices para consultas frecuentes
            entity.HasIndex(e => e.FechaReserva);
            entity.HasIndex(e => e.Estado);
            entity.HasIndex(e => new { e.FechaReserva, e.Estado });
        });

        // Configuraciones para Turno
        builder.Entity<Turno>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FechaTurno).IsRequired();
            entity.Property(e => e.HoraInicio).IsRequired();
            entity.Property(e => e.HoraFin).IsRequired();
            entity.Property(e => e.Estado).IsRequired();
            entity.Property(e => e.HorasTrabajadas).HasColumnType("decimal(5,2)");
            entity.Property(e => e.Observaciones).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Relación muchos a uno: Turnos -> Empleado
            entity.HasOne(t => t.Empleado)
                  .WithMany(e => e.Turnos)
                  .HasForeignKey(t => t.EmpleadoId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Índices para consultas frecuentes
            entity.HasIndex(e => e.FechaTurno);
            entity.HasIndex(e => new { e.EmpleadoId, e.FechaTurno });
        });

        // Configuraciones para Inventario
        builder.Entity<Inventario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Categoria).IsRequired();
            entity.Property(e => e.Unidad).HasMaxLength(20).IsRequired();
            entity.Property(e => e.PrecioCosto).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.CantidadActual).IsRequired();
            entity.Property(e => e.CantidadMinima).IsRequired();
            entity.Property(e => e.CantidadMaxima).IsRequired();
            entity.Property(e => e.Proveedor).HasMaxLength(100);
            entity.Property(e => e.NivelStock).IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Índices para consultas frecuentes
            entity.HasIndex(e => e.Categoria);
            entity.HasIndex(e => e.Nombre);
            entity.HasIndex(e => e.FechaVencimiento);
            entity.HasIndex(e => e.NivelStock);
        });

        // Configuraciones para PrediccionDemanda
        builder.Entity<PrediccionDemanda>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FechaPrediccion).IsRequired();
            entity.Property(e => e.PeriodoInicio).IsRequired();
            entity.Property(e => e.PeriodoFin).IsRequired();
            entity.Property(e => e.ReservasPronosticadas).IsRequired();
            entity.Property(e => e.PersonasEstimadas).IsRequired();
            entity.Property(e => e.IngresosEstimados).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.PromedioReservasDiarias).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.PromedioPersonasPorReserva).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.PromedioIngresoPorPersona).HasColumnType("decimal(10,2)").IsRequired();
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            // Configuración de propiedades JSON
            entity.Property(e => e.ElementosAltos).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ElementosMedios).HasColumnType("nvarchar(max)");
            entity.Property(e => e.ElementosBajos).HasColumnType("nvarchar(max)");

            // Índices para consultas frecuentes
            entity.HasIndex(e => e.PeriodoInicio);
            entity.HasIndex(e => e.FechaPrediccion);
            entity.HasIndex(e => e.CreatedAt);
        });

        // Configurar nombres de tablas para mantener consistencia
        builder.Entity<Usuario>().ToTable("Usuarios");
        builder.Entity<Empleado>().ToTable("Empleados");
        builder.Entity<Administrador>().ToTable("Administradores");
        builder.Entity<Reserva>().ToTable("Reservas");
        builder.Entity<Turno>().ToTable("Turnos");
        builder.Entity<Inventario>().ToTable("Inventarios");
        builder.Entity<PrediccionDemanda>().ToTable("PrediccionesDemanda");
    }
}