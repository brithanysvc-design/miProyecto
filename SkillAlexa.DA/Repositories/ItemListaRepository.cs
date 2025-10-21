using Microsoft.EntityFrameworkCore;
using SkillAlexa.BC.Entities;
using SkillAlexa.BC.Enums;
using SkillAlexa.BC.Interfaces;
using SkillAlexa.DA.Context;

namespace SkillAlexa.DA.Repositories;

public class ItemListaRepository:IItemListaRepository
{
    private readonly SkillAlexaDbContext _context;

    public ItemListaRepository(SkillAlexaDbContext context)
    {
        _context = context;
    }

    public async Task<ItemLista> AgregarProductoAsync(ItemLista item)
    {
        await _context.ItemsLista.AddAsync(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<ItemLista?> ObtenerProductoPorIdAsync(Guid idItem)
    {
        return await _context.ItemsLista
            .Include(i => i.Lista)
            .FirstOrDefaultAsync(i => i.IdItem == idItem);
    }

    public async Task<IEnumerable<ItemLista>> ObtenerProductosPorListaAsync(Guid idLista)
    {
        return await _context.ItemsLista
            .Where(i => i.IdLista == idLista)
            .OrderBy(i => i.Estado)
            .ThenBy(i => i.NombreProducto)
            .ToListAsync();
    }

    public async Task<ItemLista> ActualizarProductoAsync(ItemLista item)
    {
        item.FechaModificacion = DateTime.UtcNow;
        _context.ItemsLista.Update(item);
        await _context.SaveChangesAsync();
        return item;
    }

    public async Task<bool> EliminarProductoAsync(Guid idItem)
    {
        var item = await _context.ItemsLista.FindAsync(idItem);
        if (item == null)
            return false;

        _context.ItemsLista.Remove(item);
        await _context.SaveChangesAsync();
        return true;
    }
    

    public async Task<bool> CambiarEstadoProductoAsync(Guid idItem, EstadoProducto nuevoEstado)
    {
        var item = await _context.ItemsLista.FindAsync(idItem);
        if (item == null)
            return false;

        item.Estado = nuevoEstado;
        item.FechaModificacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }
}