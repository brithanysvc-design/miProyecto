using SkillAlexa.BC.Enums;

namespace SkillAlexa.BW.DTOs;

public class ItemListaDto
{
 
    public Guid IdItem { get; set; }
    public Guid IdLista { get; set; }
 
    public string NombreProducto { get; set; } = string.Empty;
 
    public decimal Cantidad { get; set; }
 
    public string? Unidad { get; set; }
    public EstadoProducto Estado { get; set; }
 
    public DateTime FechaCreacion { get; set; }
}