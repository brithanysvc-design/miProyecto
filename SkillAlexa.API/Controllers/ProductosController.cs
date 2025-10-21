using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using SkillAlexa.API.Models;
using SkillAlexa.BW.DTOs;
using SkillAlexa.BW.Services;

namespace SkillAlexa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProductosController : ControllerBase
{
    private readonly IProductoService _productoService;
    private readonly ILogger<ProductosController> _logger;
    private readonly IValidator<AgregarProductoDto> _validator;

    public ProductosController(
        IProductoService productoService,
        ILogger<ProductosController> logger,
        IValidator<AgregarProductoDto> validator)
    {
        _productoService = productoService;
        _logger = logger;
        _validator = validator;
    }

    /// <summary>
    /// Agrega un producto a una lista
    /// </summary>
    /// <param name="dto">Datos del producto a agregar</param>
    /// <returns>Producto agregado</returns>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<ItemListaDto>>> AgregarProducto([FromBody] AgregarProductoDto dto)
    {
        _logger.LogInformation("Agregando producto {Producto} a la lista {IdLista}", dto.NombreProducto, dto.IdLista);

        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errores = validationResult.Errors.Select(e => e.ErrorMessage).ToList();
            return BadRequest(ApiResponse<ItemListaDto>.Error("Errores de validación", errores));
        }

        var producto = await _productoService.AgregarProductoAsync(dto);
        return CreatedAtAction(nameof(ObtenerProductoPorId), new { id = producto.IdItem },
            ApiResponse<ItemListaDto>.Success(producto, "Producto agregado exitosamente"));
    }

    /// <summary>
    /// Obtiene un producto por su ID
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <returns>Datos del producto</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ItemListaDto>>> ObtenerProductoPorId(Guid id)
    {
        _logger.LogInformation("Obteniendo producto con ID: {Id}", id);

        var producto = await _productoService.ObtenerProductoPorIdAsync(id);
        if (producto == null)
        {
            return NotFound(ApiResponse<ItemListaDto>.Error($"No se encontró el producto con ID {id}"));
        }

        return Ok(ApiResponse<ItemListaDto>.Success(producto));
    }

    /// <summary>
    /// Obtiene todos los productos de una lista
    /// </summary>
    /// <param name="idLista">ID de la lista</param>
    /// <returns>Lista de productos</returns>
    [HttpGet("lista/{idLista}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ItemListaDto>>>> ObtenerProductosPorLista(Guid idLista)
    {
        _logger.LogInformation("Obteniendo productos de la lista: {IdLista}", idLista);

        var productos = await _productoService.ObtenerProductosPorListaAsync(idLista);
        return Ok(ApiResponse<IEnumerable<ItemListaDto>>.Success(productos));
    }

    /// <summary>
    /// Elimina un producto de una lista
    /// </summary>
    /// <param name="id">ID del producto a eliminar</param>
    /// <returns>Confirmación de eliminación</returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> EliminarProducto(Guid id)
    {
        _logger.LogInformation("Eliminando producto con ID: {Id}", id);

        var resultado = await _productoService.EliminarProductoAsync(id);
        return Ok(ApiResponse<bool>.Success(resultado, "Producto eliminado exitosamente"));
    }

    /// <summary>
    /// Cambia el estado de un producto (Pendiente/Comprado)
    /// </summary>
    /// <param name="dto">Datos del cambio de estado</param>
    /// <returns>Confirmación del cambio</returns>
    [HttpPatch("estado")]
    public async Task<ActionResult<ApiResponse<bool>>> CambiarEstadoProducto([FromBody] CambiarEstadoProductoDto dto)
    {
        _logger.LogInformation("Cambiando estado del producto {IdItem} a {Estado}", dto.IdItem, dto.NuevoEstado);

        var resultado = await _productoService.CambiarEstadoProductoAsync(dto);
        return Ok(ApiResponse<bool>.Success(resultado, "Estado del producto actualizado exitosamente"));
    }
}
