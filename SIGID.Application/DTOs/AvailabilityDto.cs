using System.ComponentModel.DataAnnotations;

namespace SIGID.Application.DTOs;

public class AvailabilityDto
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public IEnumerable<TimeSlotDto> TimeSlots { get; set; } = new List<TimeSlotDto>();
}

public class TimeSlotDto
{
    public Guid Id { get; set; }
    public string StartTime { get; set; } = string.Empty; // Format: "09:00"
    public string EndTime { get; set; } = string.Empty;   // Format: "10:00"
    public bool IsBooked { get; set; }
    public Guid? ReservaId { get; set; }
}

public class CreateAvailabilityDto
{
    [Required(ErrorMessage = "La fecha es requerida")]
    [DataType(DataType.Date)]
    public DateTime Date { get; set; }
    
    [Required(ErrorMessage = "Los horarios son requeridos")]
    [MinLength(1, ErrorMessage = "Debe incluir al menos un horario")]
    public ICollection<CreateTimeSlotDto> TimeSlots { get; set; } = new List<CreateTimeSlotDto>();
    
    [Required(ErrorMessage = "El ID del administrador es requerido")]
    public string CreatedBy { get; set; } = string.Empty;
}

public class CreateTimeSlotDto
{
    [Required(ErrorMessage = "La hora de inicio es requerida")]
    [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato de hora inválido. Use HH:mm")]
    public string StartTime { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La hora de fin es requerida")]
    [RegularExpression(@"^([0-1]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Formato de hora inválido. Use HH:mm")]
    public string EndTime { get; set; } = string.Empty;
}

public class UpdateAvailabilityDto
{
    [Required(ErrorMessage = "La fecha es requerida")]
    public DateTime Date { get; set; }
    
    [Required(ErrorMessage = "Los horarios son requeridos")]
    [MinLength(1, ErrorMessage = "Debe incluir al menos un horario")]
    public ICollection<CreateTimeSlotDto> TimeSlots { get; set; } = new List<CreateTimeSlotDto>();
}

public class CreateReservationDto
{
    [Required(ErrorMessage = "El ID del usuario es requerido")]
    public string UserId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La fecha es requerida")]
    public DateTime Date { get; set; }
    
    [Required(ErrorMessage = "El ID del horario es requerido")]
    public Guid TimeSlotId { get; set; }
    
    [Range(1, 20, ErrorMessage = "El número de personas debe estar entre 1 y 20")]
    public int NumeroPersonas { get; set; } = 1;
    
    [StringLength(500, ErrorMessage = "Los comentarios no pueden exceder 500 caracteres")]
    public string? Comentarios { get; set; }
}