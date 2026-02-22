using SIGID.Domain.Enums;

namespace SIGID.Domain.Entities;

public class Inventario
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public CategoriaInventario Categoria { get; set; }
    public int CantidadActual { get; set; }
    public int CantidadMinima { get; set; }
    public int CantidadMaxima { get; set; }
    public string Unidad { get; set; } = string.Empty; // kg, litros, unidades, etc.
    public decimal PrecioCosto { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public string? Proveedor { get; set; }
    public NivelStock NivelStock { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Calculated Properties
    public bool RequiereReposicion => CantidadActual <= CantidadMinima;
    public bool EstaVencido => FechaVencimiento.HasValue && FechaVencimiento.Value < DateTime.Now;
    
    // Navigation Properties
    public ICollection<PrediccionDemanda> PrediccionesRelacionadas { get; set; } = new List<PrediccionDemanda>();
}