using SkillAlexa.BC.Enums;

namespace SkillAlexa.BC.Entities;

public class ItemLista
{
    public Guid IdItem { get; set; }
    
    public Guid IdLista { get; set; }
    
    public string NombreProducto { get; set; } = string.Empty;
    
    public decimal Cantidad { get; set; }
    
    public string? Unidad { get; set; }
    public EstadoProducto Estado { get; set; }
    
    public DateTime FechaCreacion { get; set; }
    
    public DateTime? FechaModificacion { get; set; }
    
    public virtual ListaCompra? Lista { get; set; }
}