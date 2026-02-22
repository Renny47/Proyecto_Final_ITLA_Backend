using System.ComponentModel.DataAnnotations;
using SIGID.Domain.Enums;

namespace SIGID.Application.DTOs;

public class UsuarioDto
{
    public string Id { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    public string Email { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string FirstName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El apellido es requerido")]
    [StringLength(100, ErrorMessage = "El apellido no puede exceder 100 caracteres")]
    public string LastName { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El tipo de usuario es requerido")]
    public TipoUsuario TipoUsuario { get; set; }
    
    public DateTime? FechaNacimiento { get; set; }
    
    [Phone(ErrorMessage = "Formato de teléfono inválido")]
    public string? Telefono { get; set; }
    
    [StringLength(250, ErrorMessage = "La dirección no puede exceder 250 caracteres")]
    public string? Direccion { get; set; }
    
    public bool EsEmpleado { get; set; }
    public bool EsAdministrador { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}