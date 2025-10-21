using SkillAlexa.BC.Enums;

namespace SkillAlexa.BW.DTOs;

public class CambiarEstadoProductoDto
{
    public EstadoProducto NuevoEstado { get; set; }
    public Guid IdItem { get; set; }
}