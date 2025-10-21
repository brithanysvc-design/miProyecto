using Microsoft.EntityFrameworkCore;
using SkillAlexa.BC.Entities;
using SkillAlexa.BC.Enums;
using SkillAlexa.BC.Interfaces;
using SkillAlexa.DA.Context;

namespace SkillAlexa.DA.Repositories;

public class ListaCompraRepository:IListaCompraRepository
{
     private readonly SkillAlexaDbContext _context;

    public ListaCompraRepository(SkillAlexaDbContext context)
    {
        _context = context;
    }

    public async Task<ListaCompra> CrearListaAsync(ListaCompra lista)
    {
        await _context.ListasCompra.AddAsync(lista);
        await _context.SaveChangesAsync();
        return lista;
    }

    public async Task<ListaCompra?> ObtenerListaPorIdAsync(Guid idLista)
    {
        return await _context.ListasCompra
            .Include(l => l.Productos)
            .FirstOrDefaultAsync(l => l.IdLista == idLista);
    }

    public async Task<IEnumerable<ListaCompra>> ObtenerListasPorFechaAsync(DateTime fecha)
    {
        var fechaDate = fecha.Date;
        return await _context.ListasCompra
            .Include(l => l.Productos)
            .Where(l => l.FechaObjetivo == fechaDate && l.Estado == EstadoLista.Activa)
            .OrderBy(l => l.Nombre)
            .ToListAsync();
    }

    public async Task<IEnumerable<ListaCompra>> ObtenerListasActivasAsync()
    {
        return await _context.ListasCompra
            .Include(l => l.Productos)
            .Where(l => l.Estado == EstadoLista.Activa)
            .OrderByDescending(l => l.FechaObjetivo)
            .ThenBy(l => l.Nombre)
            .ToListAsync();
    }

    public async Task<ListaCompra> ActualizarListaAsync(ListaCompra lista)
    {
        lista.FechaModificacion = DateTime.UtcNow;
        _context.ListasCompra.Update(lista);
        await _context.SaveChangesAsync();
        return lista;
    }

    public async Task<bool> EliminarListaAsync(Guid idLista)
    {
        var lista = await _context.ListasCompra.FindAsync(idLista);
        if (lista == null)
            return false;

        lista.Estado = EstadoLista.Eliminada;
        lista.FechaModificacion = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExisteListaConNombreYFechaAsync(string nombre, DateTime fecha, Guid? idListaExcluir = null)
    {
        var fechaDate = fecha.Date;
        var query = _context.ListasCompra
            .Where(l => l.Nombre.ToLower() == nombre.ToLower() 
                && l.FechaObjetivo == fechaDate 
                && l.Estado == EstadoLista.Activa);

        if (idListaExcluir.HasValue)
        {
            query = query.Where(l => l.IdLista != idListaExcluir.Value);
        }

        return await query.AnyAsync();
    }
}