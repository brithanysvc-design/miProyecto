using SkillAlexa.BC.Entities;
using SkillAlexa.BC.Enums;

namespace SkillAlexa.BC.Interfaces;

public interface IItemListaRepository
{
    /// <summary>
    /// Agrega un producto a una lista
    /// </summary>
    Task<ItemLista> AgregarProductoAsync(ItemLista item);
    
    /// <summary>
    /// Obtiene un producto por su ID
    /// </summary>
    Task<ItemLista?> ObtenerProductoPorIdAsync(Guid idItem);
    
    /// <summary>
    /// Obtiene todos los productos de una lista específica
    /// </summary>
    Task<IEnumerable<ItemLista>> ObtenerProductosPorListaAsync(Guid idLista);
    
    /// <summary>
    /// Actualiza un producto existente
    /// </summary>
    Task<ItemLista> ActualizarProductoAsync(ItemLista item);
    
    /// <summary>
    /// Elimina un producto de una lista
    /// </summary>
    Task<bool> EliminarProductoAsync(Guid idItem);
    
    /// <summary>
    /// Cambia el estado de un producto (Pendiente/Comprado)
    /// </summary>
    Task<bool> CambiarEstadoProductoAsync(Guid idItem, EstadoProducto nuevoEstado);
}