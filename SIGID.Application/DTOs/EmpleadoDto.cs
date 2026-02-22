using System.ComponentModel.DataAnnotations;

namespace SIGID.Application.DTOs;

public class EmpleadoDto
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "El usuario ID es requerido")]
    public string UsuarioId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El puesto es requerido")]
    [StringLength(100, ErrorMessage = "El puesto no puede exceder 100 caracteres")]
    public string Puesto { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El salario por hora es requerido")]
    [Range(0.01, 999999.99, ErrorMessage = "El salario debe ser mayor a 0")]
    public decimal SalarioHora { get; set; }
    
    [Required(ErrorMessage = "La fecha de contratación es requerida")]
    public DateTime FechaContratacion { get; set; }
    
    public bool Activo { get; set; } = true;
    
    [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
    public string? Departamento { get; set; }
    
    [StringLength(100, ErrorMessage = "El supervisor no puede exceder 100 caracteres")]
    public string? Supervisor { get; set; }
    
    // Usuario info
    public string? UsuarioNombre { get; set; }
    public string? UsuarioEmail { get; set; }
}

public class CreateEmpleadoDto
{
    [Required(ErrorMessage = "El usuario ID es requerido")]
    public string UsuarioId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El puesto es requerido")]
    [StringLength(100, ErrorMessage = "El puesto no puede exceder 100 caracteres")]
    public string Puesto { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El salario por hora es requerido")]
    [Range(0.01, 999999.99, ErrorMessage = "El salario debe ser mayor a 0")]
    public decimal SalarioHora { get; set; }
    
    public DateTime FechaContratacion { get; set; } = DateTime.UtcNow;
    
    [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
    public string? Departamento { get; set; }
    
    [StringLength(100, ErrorMessage = "El supervisor no puede exceder 100 caracteres")]
    public string? Supervisor { get; set; }
}

public class UpdateEmpleadoDto
{
    [Required(ErrorMessage = "El puesto es requerido")]
    [StringLength(100, ErrorMessage = "El puesto no puede exceder 100 caracteres")]
    public string Puesto { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El salario por hora es requerido")]
    [Range(0.01, 999999.99, ErrorMessage = "El salario debe ser mayor a 0")]
    public decimal SalarioHora { get; set; }
    
    [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
    public string? Departamento { get; set; }
    
    [StringLength(100, ErrorMessage = "El supervisor no puede exceder 100 caracteres")]
    public string? Supervisor { get; set; }
    
    public bool Activo { get; set; }
}

public class CambiarEstadoEmpleadoDto
{
    [Required]
    public bool Activo { get; set; }
}