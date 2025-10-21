using SkillAlexa.BC.Entities;
using SkillAlexa.BC.Enums;
using SkillAlexa.BC.Interfaces;
using SkillAlexa.BW.DTOs;

namespace SkillAlexa.BW.Services;

public class ListaCompraService:IListaCompraService
{
     private readonly IListaCompraRepository _listaRepository;

    public ListaCompraService(IListaCompraRepository listaRepository)
    {
        _listaRepository = listaRepository;
    }

    public async Task<ListaCompraDto> CrearListaAsync(CrearListaCompraDto dto)
    {
        // Verificar si ya existe una lista con el mismo nombre en la misma fecha
        var existe = await _listaRepository.ExisteListaConNombreYFechaAsync(dto.Nombre, dto.FechaObjetivo.Date);
        if (existe)
        {
            throw new InvalidOperationException($"Ya existe una lista con el nombre '{dto.Nombre}' para la fecha {dto.FechaObjetivo:dd/MM/yyyy}");
        }

        var lista = new ListaCompra
        {
            IdLista = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            FechaObjetivo = dto.FechaObjetivo.Date,
            Estado = EstadoLista.Activa,
            FechaCreacion = DateTime.UtcNow
        };

        var listaCreada = await _listaRepository.CrearListaAsync(lista);
        return MapearADto(listaCreada);
    }

    public async Task<ListaCompraDto?> ObtenerListaPorIdAsync(Guid idLista)
    {
        var lista = await _listaRepository.ObtenerListaPorIdAsync(idLista);
        return lista != null ? MapearADto(lista) : null;
    }

    public async Task<IEnumerable<ListaCompraDto>> ObtenerListasPorFechaAsync(DateTime fecha)
    {
        var listas = await _listaRepository.ObtenerListasPorFechaAsync(fecha.Date);
        return listas.Select(MapearADto);
    }

    public async Task<IEnumerable<ListaCompraDto>> ObtenerListasActivasAsync()
    {
        var listas = await _listaRepository.ObtenerListasActivasAsync();
        return listas.Select(MapearADto);
    }

    public async Task<bool> EliminarListaAsync(Guid idLista)
    {
        var lista = await _listaRepository.ObtenerListaPorIdAsync(idLista);
        if (lista == null)
        {
            throw new KeyNotFoundException($"No se encontró la lista con ID {idLista}");
        }

        if (lista.Estado == EstadoLista.Eliminada)
        {
            throw new InvalidOperationException("La lista ya está eliminada");
        }

        return await _listaRepository.EliminarListaAsync(idLista);
    }

    private ListaCompraDto MapearADto(ListaCompra lista)
    {
        return new ListaCompraDto
        {
            IdLista = lista.IdLista,
            Nombre = lista.Nombre,
            FechaObjetivo = lista.FechaObjetivo,
            Estado = lista.Estado,
            FechaCreacion = lista.FechaCreacion,
            Productos = lista.Productos?.Select(p => new ItemListaDto
            {
                IdItem = p.IdItem,
                IdLista = p.IdLista,
                NombreProducto = p.NombreProducto,
                Cantidad = p.Cantidad,
                Unidad = p.Unidad,
                Estado = p.Estado,
                FechaCreacion = p.FechaCreacion
            }).ToList() ?? new List<ItemListaDto>()
        };
    }
}