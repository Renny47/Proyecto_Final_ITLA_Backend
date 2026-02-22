using System.ComponentModel.DataAnnotations;
using SIGID.Domain.Enums;

namespace SIGID.Application.DTOs;

public class ReservaDto
{
    public Guid Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public DateTime FechaReserva { get; set; }
    public DateTime FechaHoraReserva { get; set; }
    public int NumeroPersonas { get; set; }
    public EstadoReserva Estado { get; set; }
    public string? Comentarios { get; set; }
    public string? NumeroMesa { get; set; }
    public decimal? MontoEstimado { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Usuario info
    public string? UsuarioNombre { get; set; }
    public string? UsuarioEmail { get; set; }
}

public class CreateReservaDto
{
    [Required(ErrorMessage = "El usuario ID es requerido")]
    public string UsuarioId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La fecha y hora de reserva es requerida")]
    [DataType(DataType.DateTime, ErrorMessage = "Formato de fecha inválido")]
    public DateTime FechaHoraReserva { get; set; }
    
    [Required(ErrorMessage = "El número de personas es requerido")]
    [Range(1, 20, ErrorMessage = "El número de personas debe estar entre 1 y 20")]
    public int NumeroPersonas { get; set; }
    
    [StringLength(500, ErrorMessage = "Los comentarios no pueden exceder 500 caracteres")]
    public string? Comentarios { get; set; }
    
    [StringLength(10, ErrorMessage = "El número de mesa no puede exceder 10 caracteres")]
    public string? NumeroMesa { get; set; }
    
    [Range(0.01, 99999.99, ErrorMessage = "El monto estimado debe ser mayor a 0")]
    public decimal? MontoEstimado { get; set; }
}

public class UpdateReservaDto
{
    [Required(ErrorMessage = "La fecha y hora de reserva es requerida")]
    public DateTime FechaHoraReserva { get; set; }
    
    [Required(ErrorMessage = "El número de personas es requerido")]
    [Range(1, 20, ErrorMessage = "El número de personas debe estar entre 1 y 20")]
    public int NumeroPersonas { get; set; }
    
    [Required(ErrorMessage = "El estado es requerido")]
    public EstadoReserva Estado { get; set; }
    
    [StringLength(500, ErrorMessage = "Los comentarios no pueden exceder 500 caracteres")]
    public string? Comentarios { get; set; }
    
    [StringLength(10, ErrorMessage = "El número de mesa no puede exceder 10 caracteres")]
    public string? NumeroMesa { get; set; }
    
    [Range(0.01, 99999.99, ErrorMessage = "El monto estimado debe ser mayor a 0")]
    public decimal? MontoEstimado { get; set; }
}