using SkillAlexa.BC.Enums;

namespace SkillAlexa.BW.DTOs;

public class ListaCompraDto
{
    public Guid IdLista { get; set; }
    
    public string Nombre { get; set; } = string.Empty;

    public DateTime FechaObjetivo { get; set; }

    public EstadoLista Estado { get; set; }

    public DateTime FechaCreacion { get; set; }
    
    public List<ItemListaDto> Productos { get; set; } = new();
}