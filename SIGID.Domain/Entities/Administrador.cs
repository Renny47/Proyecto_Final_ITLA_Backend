using SIGID.Domain.Enums;

namespace SIGID.Domain.Entities;

public class Administrador
{
    public Guid Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty; // Foreign Key to Usuario/User
    public string NivelAcceso { get; set; } = string.Empty;
    public DateTime FechaAsignacion { get; set; }
    public bool Activo { get; set; } = true;
    public string? AreaResponsabilidad { get; set; }
    
    // Navigation Properties
    public Usuario Usuario { get; set; } = null!;
}