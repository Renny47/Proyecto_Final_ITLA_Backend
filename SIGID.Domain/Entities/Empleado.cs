using SIGID.Domain.Enums;

namespace SIGID.Domain.Entities;

public class Empleado
{
    public Guid Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty; // Foreign Key to Usuario/User
    public string Puesto { get; set; } = string.Empty;
    public decimal SalarioHora { get; set; }
    public DateTime FechaContratacion { get; set; }
    public bool Activo { get; set; } = true;
    public string? Departamento { get; set; }
    public string? Supervisor { get; set; }
    
    // Navigation Properties
    public Usuario Usuario { get; set; } = null!;
    public ICollection<Turno> Turnos { get; set; } = new List<Turno>();
}