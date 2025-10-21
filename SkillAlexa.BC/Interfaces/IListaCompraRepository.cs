using SkillAlexa.BC.Entities;

namespace SkillAlexa.BC.Interfaces;

public interface IListaCompraRepository
{
    /// <summary>
    /// Crea una nueva lista de compras
    /// </summary>
    Task<ListaCompra> CrearListaAsync(ListaCompra lista);
    
    /// <summary>
    /// Obtiene una lista por su ID
    /// </summary>
    Task<ListaCompra?> ObtenerListaPorIdAsync(Guid idLista);
    
    /// <summary>
    /// Obtiene todas las listas activas de una fecha específica
    /// </summary>
    Task<IEnumerable<ListaCompra>> ObtenerListasPorFechaAsync(DateTime fecha);
    
    /// <summary>
    /// Obtiene todas las listas activas
    /// </summary>
    Task<IEnumerable<ListaCompra>> ObtenerListasActivasAsync();
    
    /// <summary>
    /// Actualiza una lista existente
    /// </summary>
    Task<ListaCompra> ActualizarListaAsync(ListaCompra lista);
    
    /// <summary>
    /// Elimina lógicamente una lista
    /// </summary>
    Task<bool> EliminarListaAsync(Guid idLista);
    
    /// <summary>
    /// Verifica si existe una lista con el mismo nombre en la misma fecha
    /// </summary>
    Task<bool> ExisteListaConNombreYFechaAsync(string nombre, DateTime fecha, Guid? idListaExcluir = null);
}