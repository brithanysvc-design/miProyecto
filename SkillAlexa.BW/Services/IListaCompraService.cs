using SkillAlexa.BW.DTOs;

namespace SkillAlexa.BW.Services;

public interface IListaCompraService
{
    /// <summary>
    /// Crea una nueva lista de compras
    /// </summary>
    Task<ListaCompraDto> CrearListaAsync(CrearListaCompraDto dto);
    
    /// <summary>
    /// Obtiene una lista por su ID
    /// </summary>
    Task<ListaCompraDto?> ObtenerListaPorIdAsync(Guid idLista);
    
    /// <summary>
    /// Obtiene todas las listas de una fecha específica
    /// </summary>
    Task<IEnumerable<ListaCompraDto>> ObtenerListasPorFechaAsync(DateTime fecha);
    
    /// <summary>
    /// Obtiene todas las listas activas
    /// </summary>
    Task<IEnumerable<ListaCompraDto>> ObtenerListasActivasAsync();
    
    /// <summary>
    /// Elimina una lista
    /// </summary>
    Task<bool> EliminarListaAsync(Guid idLista);
}