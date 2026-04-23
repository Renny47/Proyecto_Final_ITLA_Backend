namespace SIGID.Domain.Entities;

public class Availability
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string CreatedBy { get; set; } = string.Empty; // Admin ID
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation Properties
    public ICollection<TimeSlot> TimeSlots { get; set; } = new List<TimeSlot>();
}