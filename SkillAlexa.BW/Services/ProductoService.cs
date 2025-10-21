using SkillAlexa.BC.Entities;
using SkillAlexa.BC.Enums;
using SkillAlexa.BC.Interfaces;
using SkillAlexa.BW.DTOs;

namespace SkillAlexa.BW.Services;

public class ProductoService:IProductoService
{
     private readonly IItemListaRepository _itemRepository;
    private readonly IListaCompraRepository _listaRepository;

    public ProductoService(IItemListaRepository itemRepository, IListaCompraRepository listaRepository)
    {
        _itemRepository = itemRepository;
        _listaRepository = listaRepository;
    }

    public async Task<ItemListaDto> AgregarProductoAsync(AgregarProductoDto dto)
    {
        // Verificar que la lista existe y está activa
        var lista = await _listaRepository.ObtenerListaPorIdAsync(dto.IdLista);
        if (lista == null)
        {
            throw new KeyNotFoundException($"No se encontró la lista con ID {dto.IdLista}");
        }

        if (lista.Estado == EstadoLista.Eliminada)
        {
            throw new InvalidOperationException("No se pueden agregar productos a una lista eliminada");
        }

        var item = new ItemLista
        {
            IdItem = Guid.NewGuid(),
            IdLista = dto.IdLista,
            NombreProducto = dto.NombreProducto.Trim(),
            Cantidad = dto.Cantidad,
            Unidad = dto.Unidad?.Trim(),
            Estado = EstadoProducto.Pendiente,
            FechaCreacion = DateTime.UtcNow
        };

        var itemCreado = await _itemRepository.AgregarProductoAsync(item);
        return MapearADto(itemCreado);
    }

    public async Task<ItemListaDto?> ObtenerProductoPorIdAsync(Guid idItem)
    {
        var item = await _itemRepository.ObtenerProductoPorIdAsync(idItem);
        return item != null ? MapearADto(item) : null;
    }

    public async Task<IEnumerable<ItemListaDto>> ObtenerProductosPorListaAsync(Guid idLista)
    {
        var items = await _itemRepository.ObtenerProductosPorListaAsync(idLista);
        return items.Select(MapearADto);
    }

    public async Task<bool> EliminarProductoAsync(Guid idItem)
    {
        var item = await _itemRepository.ObtenerProductoPorIdAsync(idItem);
        if (item == null)
        {
            throw new KeyNotFoundException($"No se encontró el producto con ID {idItem}");
        }

        return await _itemRepository.EliminarProductoAsync(idItem);
    }

    public async Task<bool> CambiarEstadoProductoAsync(CambiarEstadoProductoDto dto)
    {
        var item = await _itemRepository.ObtenerProductoPorIdAsync(dto.IdItem);
        if (item == null)
        {
            throw new KeyNotFoundException($"No se encontró el producto con ID {dto.IdItem}");
        }

        return await _itemRepository.CambiarEstadoProductoAsync(dto.IdItem, dto.NuevoEstado);
    }

    private ItemListaDto MapearADto(ItemLista item)
    {
        return new ItemListaDto
        {
            IdItem = item.IdItem,
            IdLista = item.IdLista,
            NombreProducto = item.NombreProducto,
            Cantidad = item.Cantidad,
            Unidad = item.Unidad,
            Estado = item.Estado,
            FechaCreacion = item.FechaCreacion
        };
    }
}