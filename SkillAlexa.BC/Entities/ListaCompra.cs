using SkillAlexa.BC.Enums;

namespace SkillAlexa.BC.Entities;

public class ListaCompra
{
    public Guid IdLista { get; set; }
    
    public string Nombre { get; set; } = string.Empty;
    
    public DateTime FechaObjetivo { get; set; }
    
    public EstadoLista Estado { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaModificacion { get; set; }

    public virtual ICollection<ItemLista> Productos { get; set; } = new List<ItemLista>();
}