using System.ComponentModel.DataAnnotations;
using SIGID.Domain.Enums;

namespace SIGID.Application.DTOs;

public class InventarioDto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public CategoriaInventario Categoria { get; set; }
    public int CantidadActual { get; set; }
    public int CantidadMinima { get; set; }
    public int CantidadMaxima { get; set; }
    public string Unidad { get; set; } = string.Empty;
    public decimal PrecioCosto { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public string? Proveedor { get; set; }
    public NivelStock NivelStock { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool RequiereReposicion { get; set; }
    public bool EstaVencido { get; set; }
}

public class CreateInventarioDto
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
    
    [Required(ErrorMessage = "La categoría es requerida")]
    public CategoriaInventario Categoria { get; set; }
    
    [Required(ErrorMessage = "La cantidad actual es requerida")]
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad actual no puede ser negativa")]
    public int CantidadActual { get; set; }
    
    [Required(ErrorMessage = "La cantidad mínima es requerida")]
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad mínima no puede ser negativa")]
    public int CantidadMinima { get; set; }
    
    [Required(ErrorMessage = "La cantidad máxima es requerida")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad máxima debe ser mayor a 0")]
    public int CantidadMaxima { get; set; }
    
    [Required(ErrorMessage = "La unidad es requerida")]
    [StringLength(50, ErrorMessage = "La unidad no puede exceder 50 caracteres")]
    public string Unidad { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El precio de costo es requerido")]
    [Range(0.01, 99999.99, ErrorMessage = "El precio de costo debe ser mayor a 0")]
    public decimal PrecioCosto { get; set; }
    
    [DataType(DataType.Date, ErrorMessage = "Formato de fecha inválido")]
    public DateTime? FechaVencimiento { get; set; }
    
    [StringLength(100, ErrorMessage = "El proveedor no puede exceder 100 caracteres")]
    public string? Proveedor { get; set; }
}

public class UpdateInventarioDto
{
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    public string Nombre { get; set; } = string.Empty;
    
    [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres")]
    public string? Descripcion { get; set; }
    
    [Required(ErrorMessage = "La categoría es requerida")]
    public CategoriaInventario Categoria { get; set; }
    
    [Required(ErrorMessage = "La cantidad actual es requerida")]
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad actual no puede ser negativa")]
    public int CantidadActual { get; set; }
    
    [Required(ErrorMessage = "La cantidad mínima es requerida")]
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad mínima no puede ser negativa")]
    public int CantidadMinima { get; set; }
    
    [Required(ErrorMessage = "La cantidad máxima es requerida")]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad máxima debe ser mayor a 0")]
    public int CantidadMaxima { get; set; }
    
    [Required(ErrorMessage = "La unidad es requerida")]
    [StringLength(50, ErrorMessage = "La unidad no puede exceder 50 caracteres")]
    public string Unidad { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "El precio de costo es requerido")]
    [Range(0.01, 99999.99, ErrorMessage = "El precio de costo debe ser mayor a 0")]
    public decimal PrecioCosto { get; set; }
    
    public DateTime? FechaVencimiento { get; set; }
    
    [StringLength(100, ErrorMessage = "El proveedor no puede exceder 100 caracteres")]
    public string? Proveedor { get; set; }
    
    [Required(ErrorMessage = "El nivel de stock es requerido")]
    public NivelStock NivelStock { get; set; }
}

public class UpdateStockDto
{
    [Required(ErrorMessage = "La nueva cantidad es requerida")]
    [Range(0, int.MaxValue, ErrorMessage = "La cantidad no puede ser negativa")]
    public int NuevaCantidad { get; set; }
    
    [StringLength(200, ErrorMessage = "La razón no puede exceder 200 caracteres")]
    public string? Razon { get; set; }
}