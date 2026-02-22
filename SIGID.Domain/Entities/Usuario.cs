using Microsoft.AspNetCore.Identity;
using SIGID.Domain.Enums;

namespace SIGID.Domain.Entities;

public class Usuario : IdentityUser
{
    // Propiedades que estaban en User
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Propiedades propias de Usuario
    public TipoUsuario TipoUsuario { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public bool EsEmpleado { get; set; }
    public bool EsAdministrador { get; set; }

    // Navigation Properties
    public ICollection<Reserva> Reservas { get; set; } = new List<Reserva>();
    public Empleado? Empleado { get; set; }
    public Administrador? Administrador { get; set; }
}
