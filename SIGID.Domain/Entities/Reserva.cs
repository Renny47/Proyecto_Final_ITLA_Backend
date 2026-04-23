using SIGID.Domain.Enums;

namespace SIGID.Domain.Entities;

public class Reserva
{
    public Guid Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty; // Foreign Key to Usuario
    public DateTime FechaReserva { get; set; }
    public DateTime FechaHoraReserva { get; set; }
    public int NumeroPersonas { get; set; }
    public EstadoReserva Estado { get; set; }
    public string? Comentarios { get; set; }
    public string? NumeroMesa { get; set; }
    public decimal? MontoEstimado { get; set; }
    public Guid? TimeSlotId { get; set; } // Foreign Key to TimeSlot (nullable for backward compatibility)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public Usuario Usuario { get; set; } = null!;
    public TimeSlot? TimeSlot { get; set; }
}