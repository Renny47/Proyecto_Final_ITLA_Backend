namespace SIGID.Domain.Entities;

public class TimeSlot
{
    public Guid Id { get; set; }
    public Guid AvailabilityId { get; set; } // Foreign Key
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsBooked { get; set; } = false;
    public Guid? ReservaId { get; set; } // Foreign Key to Reserva (nullable)
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public Availability Availability { get; set; } = null!;
    public Reserva? Reserva { get; set; }
}