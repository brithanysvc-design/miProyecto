using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SkillAlexa.API.Models;
using SkillAlexa.BW.DTOs;
using SkillAlexa.BW.Services;

namespace SkillAlexa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ListasController : ControllerBase
{
    private readonly IListaCompraService _listaService;
    private readonly ILogger<ListasController> _logger;
    private readonly IValidator<CrearListaCompraDto> _validator;

    public ListasController(
        IListaCompraService listaService,
        ILogger<ListasController> logger,
        IValidator<CrearListaCompraDto> validator)
    {
        _listaService = listaService;
        _logger = logger;
        _validator = validator;
    }

    /// <summary>
    /// Crea una nueva lista de compras
    /// </summary>
    /// <param name="dto">Datos de la lista a crear</param>
    /// <returns>Lista creada</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ListaCompraDto>>> CrearLista([FromBody] CrearListaCompraDto dto)
    {
        _logger.LogInformation("Creando nueva lista de compras: {Nombre}", dto.Nombre);

        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errores = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ListaCompraDto>.Error("Errores de validación", errores));
        }

        var lista = await _listaService.CrearListaAsync(dto);
        return CreatedAtAction(nameof(ObtenerListaPorId), new { id = lista.IdLista }, 
            ApiResponse<ListaCompraDto>.Success(lista, "Lista creada exitosamente"));
    }

    /// <summary>
    /// Obtiene una lista por su ID
    /// </summary>
    /// <param name="id">ID de la lista</param>
    /// <returns>Datos de la lista</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ListaCompraDto>>> ObtenerListaPorId(Guid id)
    {
        _logger.LogInformation("Obteniendo lista con ID: {Id}", id);

        var lista = await _listaService.ObtenerListaPorIdAsync(id);
        if (lista == null)
        {
            return NotFound(ApiResponse<ListaCompraDto>.Error($"No se encontró la lista con ID {id}"));
        }

        return Ok(ApiResponse<ListaCompraDto>.Success(lista));
    }

    /// <summary>
    /// Obtiene todas las listas de una fecha específica
    /// </summary>
    /// <param name="fecha">Fecha a consultar (formato: yyyy-MM-dd)</param>
    /// <returns>Lista de listas de compras</returns>
    [HttpGet("por-fecha")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ListaCompraDto>>>> ObtenerListasPorFecha([FromQuery] DateTime fecha)
    {
        _logger.LogInformation("Obteniendo listas para la fecha: {Fecha}", fecha);

        var listas = await _listaService.ObtenerListasPorFechaAsync(fecha);
        return Ok(ApiResponse<IEnumerable<ListaCompraDto>>.Success(listas));
    }

    /// <summary>
    /// Obtiene todas las listas activas
    /// </summary>
    /// <returns>Lista de listas activas</returns>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ListaCompraDto>>>> ObtenerListasActivas()
    {
        _logger.LogInformation("Obteniendo todas las listas activas");

        var listas = await _listaService.ObtenerListasActivasAsync();
        return Ok(ApiResponse<IEnumerable<ListaCompraDto>>.Success(listas));
    }

    /// <summary>
    /// Elimina una lista de compras (soft delete)
    /// </summary>
    /// <param name="id">ID de la lista a eliminar</param>
    /// <returns>Confirmación de eliminación</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> EliminarLista(Guid id)
    {
        _logger.LogInformation("Eliminando lista con ID: {Id}", id);

        var resultado = await _listaService.EliminarListaAsync(id);
        return Ok(ApiResponse<bool>.Success(resultado, "Lista eliminada exitosamente"));
    }
}