using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SIGID.Domain.Entities;

namespace SIGID.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<User>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuraciones adicionales para User
        builder.Entity<User>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.UserName).IsUnique();
        });

        builder.Entity<Booking>(entity =>
        {
            entity.Property(e => e.Id).IsRequired();
            entity.Property(e => e.BookingState).IsRequired();
            entity.Property(e => e.DateAndTime).IsRequired();
            entity.Property(e => e.BookedByClientName).HasMaxLength(100);
        });

        // Configurar nombres de tablas si es necesario
        builder.Entity<User>().ToTable("Users");
        builder.Entity<Booking>().ToTable("Bookings");
    }
}