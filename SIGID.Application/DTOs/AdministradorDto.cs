using System.ComponentModel.DataAnnotations;

namespace SIGID.Application.DTOs;

public class AdministradorDto
{
    public Guid Id { get; set; }
    public string UsuarioId { get; set; } = string.Empty;
    public string NivelAcceso { get; set; } = string.Empty;
    public DateTime FechaAsignacion { get; set; }
    public bool Activo { get; set; }
    public string? AreaResponsabilidad { get; set; }
    
    // Propiedades del Usuario relacionado
    public string? UsuarioNombre { get; set; }
    public string? UsuarioEmail { get; set; }
    public string? UsuarioTelefono { get; set; }
}

public class CreateAdministradorDto
{
    [Required(ErrorMessage = "El ID del usuario es requerido")]
    public string UsuarioId { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El nivel de acceso es requerido")]
    [StringLength(20, ErrorMessage = "El nivel de acceso no puede exceder los 20 caracteres")]
    public string NivelAcceso { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "El área de responsabilidad no puede exceder los 100 caracteres")]
    public string? AreaResponsabilidad { get; set; }
    
    public bool Activo { get; set; } = true;
}

public class UpdateAdministradorDto
{
    [Required(ErrorMessage = "El nivel de acceso es requerido")]
    [StringLength(20, ErrorMessage = "El nivel de acceso no puede exceder los 20 caracteres")]
    public string NivelAcceso { get; set; } = string.Empty;
    
    [StringLength(100, ErrorMessage = "El área de responsabilidad no puede exceder los 100 caracteres")]
    public string? AreaResponsabilidad { get; set; }
    
    public bool Activo { get; set; }
}

public class CambiarEstadoAdministradorDto
{
    [Required]
    public bool Activo { get; set; }
}

public class ConfiguracionRestauranteDto
{
    [Range(1, 1000, ErrorMessage = "La capacidad máxima debe estar entre 1 y 1000")]
    public int CapacidadMaxima { get; set; }
    
    [Range(0, 23, ErrorMessage = "La hora de apertura debe estar entre 0 y 23")]
    public int HoraApertura { get; set; }
    
    [Range(0, 23, ErrorMessage = "La hora de cierre debe estar entre 0 y 23")]
    public int HoraCierre { get; set; }
    
    [Range(15, 300, ErrorMessage = "El tiempo por mesa debe estar entre 15 y 300 minutos")]
    public int TiempoPromedioMesa { get; set; }
    
    [Range(0, 100, ErrorMessage = "El margen de ganancia debe estar entre 0 y 100")]
    public decimal MargenGanancia { get; set; }
    
    [StringLength(200, ErrorMessage = "Las observaciones no pueden exceder los 200 caracteres")]
    public string? Observaciones { get; set; }
}