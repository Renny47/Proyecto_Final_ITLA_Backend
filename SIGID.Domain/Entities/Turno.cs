using SIGID.Domain.Enums;

namespace SIGID.Domain.Entities;

public class Turno
{
    public Guid Id { get; set; }
    public Guid EmpleadoId { get; set; } // Foreign Key to Empleado
    public DateTime FechaTurno { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public EstadoTurno Estado { get; set; }
    public decimal? HorasTrabajadas { get; set; }
    public string? Observaciones { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public Empleado Empleado { get; set; } = null!;
}