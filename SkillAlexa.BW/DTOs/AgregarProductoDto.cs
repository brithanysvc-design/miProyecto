namespace SkillAlexa.BW.DTOs;

public class AgregarProductoDto
{
    public Guid IdLista { get; set; }
     
    public string NombreProducto { get; set; } = string.Empty;
    
 
    public decimal Cantidad { get; set; }
 
    public string? Unidad { get; set; }
}