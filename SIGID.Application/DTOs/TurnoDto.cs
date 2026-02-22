using System.ComponentModel.DataAnnotations;
using SIGID.Domain.Enums;

namespace SIGID.Application.DTOs;

public class TurnoDto
{
    public Guid Id { get; set; }
    public Guid EmpleadoId { get; set; }
    public DateTime FechaTurno { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public EstadoTurno Estado { get; set; }
    public decimal? HorasTrabajadas { get; set; }
    public string? Observaciones { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Empleado info
    public string? EmpleadoNombre { get; set; }
    public string? EmpleadoPuesto { get; set; }
}

public class CreateTurnoDto
{
    [Required(ErrorMessage = "El empleado ID es requerido")]
    public Guid EmpleadoId { get; set; }
    
    [Required(ErrorMessage = "La fecha del turno es requerida")]
    [DataType(DataType.Date, ErrorMessage = "Formato de fecha inválido")]
    public DateTime FechaTurno { get; set; }
    
    [Required(ErrorMessage = "La hora de inicio es requerida")]
    public TimeOnly HoraInicio { get; set; }
    
    [Required(ErrorMessage = "La hora de fin es requerida")]
    public TimeOnly HoraFin { get; set; }
    
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
}

public class UpdateTurnoDto
{
    [Required(ErrorMessage = "La fecha del turno es requerida")]
    public DateTime FechaTurno { get; set; }
    
    [Required(ErrorMessage = "La hora de inicio es requerida")]
    public TimeOnly HoraInicio { get; set; }
    
    [Required(ErrorMessage = "La hora de fin es requerida")]
    public TimeOnly HoraFin { get; set; }
    
    [Required(ErrorMessage = "El estado es requerido")]
    public EstadoTurno Estado { get; set; }
    
    [Range(0.01, 24.00, ErrorMessage = "Las horas trabajadas deben estar entre 0.01 y 24")]
    public decimal? HorasTrabajadas { get; set; }
    
    [StringLength(500, ErrorMessage = "Las observaciones no pueden exceder 500 caracteres")]
    public string? Observaciones { get; set; }
}