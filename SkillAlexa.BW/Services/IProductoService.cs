using SkillAlexa.BW.DTOs;

namespace SkillAlexa.BW.Services;

public interface IProductoService
{
    /// <summary>
    /// Agrega un producto a una lista
    /// </summary>
    Task<ItemListaDto> AgregarProductoAsync(AgregarProductoDto dto);
    
    /// <summary>
    /// Obtiene un producto por su ID
    /// </summary>
    Task<ItemListaDto?> ObtenerProductoPorIdAsync(Guid idItem);
    
    /// <summary>
    /// Obtiene todos los productos de una lista
    /// </summary>
    Task<IEnumerable<ItemListaDto>> ObtenerProductosPorListaAsync(Guid idLista);
    
    /// <summary>
    /// Elimina un producto
    /// </summary>
    Task<bool> EliminarProductoAsync(Guid idItem);
    
    /// <summary>
    /// Cambia el estado de un producto
    /// </summary>
    Task<bool> CambiarEstadoProductoAsync(CambiarEstadoProductoDto dto);
}