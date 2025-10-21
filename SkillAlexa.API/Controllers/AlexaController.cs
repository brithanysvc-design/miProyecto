using Microsoft.AspNetCore.Mvc;
using SkillAlexa.BW.DTOs;
using SkillAlexa.BW.Services;

namespace SkillAlexa.API.Controllers;
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AlexaController:ControllerBase
{
      private readonly IListaCompraService _listaService;
    private readonly IProductoService _productoService;
    private readonly ILogger<AlexaController> _logger;

    public AlexaController(
        IListaCompraService listaService,
        IProductoService productoService,
        ILogger<AlexaController> logger)
    {
        _listaService = listaService;
        _productoService = productoService;
        _logger = logger;
    }

    /// <summary>
    /// Endpoint principal para recibir requests de Alexa (JSON o texto plano)
    /// </summary>
    /// <param name="request">Request de Alexa en formato JSON o texto plano</param>
    /// <returns>Response en formato Alexa</returns>
    [HttpPost]
    public async Task<IActionResult> HandleAlexaRequest([FromBody] System.Text.Json.JsonElement request)
    {
        try
        {
            // Log del contenido recibido para debugging
            var rawText = request.GetRawText();
            _logger.LogInformation("Request recibido: {RequestJson}", rawText);

            // Verificar si es texto plano (string)
            if (request.ValueKind == System.Text.Json.JsonValueKind.String)
            {
                var command = request.GetString();
                return await ProcessTextCommand(command ?? "");
            }

            // Extraer el tipo de request de forma segura
            string requestType = "";

            // Verificar si el request tiene la estructura esperada de Alexa
            if (request.ValueKind != System.Text.Json.JsonValueKind.Object)
            {
                _logger.LogError("El request no es un objeto JSON válido. ValueKind: {ValueKind}", request.ValueKind);
                return BadRequest(new { error = "Request JSON inválido. Debe ser un objeto JSON de Alexa o texto plano." });
            }

            if (request.TryGetProperty("request", out var requestObj))
            {
                if (requestObj.ValueKind == System.Text.Json.JsonValueKind.Object)
                {
                    if (requestObj.TryGetProperty("type", out var typeObj) &&
                        typeObj.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        requestType = typeObj.GetString() ?? "";
                    }
                }
                else
                {
                    _logger.LogError("La propiedad 'request' no es un objeto. ValueKind: {ValueKind}", requestObj.ValueKind);
                }
            }
            else
            {
                _logger.LogWarning("El request no contiene la propiedad 'request'. Propiedades disponibles: {Properties}",
                    string.Join(", ", request.EnumerateObject().Select(p => p.Name)));
            }

            _logger.LogInformation("Recibiendo request de Alexa: {RequestType}", requestType);

            // Manejar diferentes tipos de requests
            return requestType switch
            {
                "LaunchRequest" => HandleLaunchRequest(),
                "IntentRequest" => await HandleIntentRequest(request),
                "SessionEndedRequest" => HandleSessionEndedRequest(),
                _ => BadRequest(new { error = $"Tipo de request no soportado: {requestType}" })
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error procesando request de Alexa");
            return StatusCode(500, BuildAlexaResponse(
                "Lo siento, ocurrió un error procesando tu solicitud. Por favor intenta nuevamente.",
                shouldEndSession: true
            ));
        }
    }

    private async Task<IActionResult> ProcessTextCommand(string command)
    {
        _logger.LogInformation("Comando de texto recibido: {Command}", command);

        if (string.IsNullOrWhiteSpace(command))
        {
            return BadRequest(new { error = "El comando no puede estar vacío" });
        }

        // Normalizar el comando (quitar "Alexa," del inicio si existe)
        var normalizedCommand = command.ToLower().Trim();
        normalizedCommand = normalizedCommand.Replace("alexa,", "").Replace("alexa", "").Trim();

        // LAUNCH REQUEST - Abrir la skill
        if (normalizedCommand.Contains("abre lista de compras") ||
            normalizedCommand.Contains("abrir lista de compras") ||
            normalizedCommand.Contains("iniciar lista de compras"))
        {
            return HandleLaunchRequest();
        }

        // CREAR LISTA - CrearListaIntent
        if (normalizedCommand.Contains("crea una lista") ||
            normalizedCommand.Contains("crear lista") ||
            normalizedCommand.Contains("nueva lista") ||
            normalizedCommand.Contains("quiero crear una lista") ||
            normalizedCommand.Contains("agrega una nueva lista") ||
            normalizedCommand.Contains("inicia una lista"))
        {
            var nombreLista = ExtractListName(normalizedCommand);
            return await HandleCrearListaFromText(nombreLista);
        }

        // ELIMINAR LISTA - EliminarListaIntent
        if (normalizedCommand.Contains("elimina la lista") ||
            normalizedCommand.Contains("borra la lista") ||
            normalizedCommand.Contains("quita la lista") ||
            normalizedCommand.Contains("eliminar lista") ||
            normalizedCommand.Contains("borrar la lista") ||
            normalizedCommand.Contains("quiero borrar la lista"))
        {
            var nombreLista = ExtractListNameToDelete(normalizedCommand);
            return await HandleEliminarListaFromText(nombreLista);
        }

        // CONSULTAR LISTAS - ListarListasIntent
        if (normalizedCommand.Contains("qué listas tengo") ||
            normalizedCommand.Contains("que listas tengo") ||
            normalizedCommand.Contains("muéstrame mis listas") ||
            normalizedCommand.Contains("cuáles son mis listas") ||
            normalizedCommand.Contains("cuales son mis listas") ||
            normalizedCommand.Contains("lista mis listas") ||
            normalizedCommand.Contains("listas de hoy") ||
            normalizedCommand.Contains("mis listas"))
        {
            return await HandleListarListasIntent(new System.Text.Json.JsonElement());
        }

        // VER PRODUCTOS - ListarProductosIntent
        if (normalizedCommand.Contains("qué hay en") ||
            normalizedCommand.Contains("que hay en") ||
            normalizedCommand.Contains("qué productos tengo") ||
            normalizedCommand.Contains("que productos tengo") ||
            normalizedCommand.Contains("lista los productos") ||
            normalizedCommand.Contains("cuáles son los productos") ||
            normalizedCommand.Contains("cuales son los productos") ||
            normalizedCommand.Contains("qué necesito comprar") ||
            normalizedCommand.Contains("que necesito comprar") ||
            normalizedCommand.Contains("lee mi lista"))
        {
            return await HandleListarProductosIntent(new System.Text.Json.JsonElement());
        }

        // MARCAR COMO COMPRADO - MarcarProductoIntent
        if (normalizedCommand.Contains("marca") && normalizedCommand.Contains("comprado") ||
            normalizedCommand.Contains("está comprado") ||
            normalizedCommand.Contains("esta comprado") ||
            normalizedCommand.Contains("cambiar") && normalizedCommand.Contains("a comprado") ||
            normalizedCommand.Contains("ya está comprado") ||
            normalizedCommand.Contains("ya esta comprado") ||
            normalizedCommand.Contains("compré") ||
            normalizedCommand.Contains("compre") ||
            normalizedCommand.Contains("ya compré") ||
            normalizedCommand.Contains("ya compre"))
        {
            var producto = ExtractProductNameForMark(normalizedCommand);
            return await HandleMarcarProductoFromText(producto);
        }

        // ELIMINAR PRODUCTO - EliminarProductoIntent
        if (normalizedCommand.Contains("elimina") && !normalizedCommand.Contains("lista") ||
            normalizedCommand.Contains("borra") && !normalizedCommand.Contains("lista") ||
            normalizedCommand.Contains("quita") && !normalizedCommand.Contains("lista") ||
            normalizedCommand.Contains("quiero quitar"))
        {
            var producto = ExtractProductNameToDelete(normalizedCommand);
            return await HandleEliminarProductoFromText(producto);
        }

        // AGREGAR PRODUCTO - AgregarProductoIntent
        if (normalizedCommand.Contains("agrega") ||
            normalizedCommand.Contains("añade") ||
            normalizedCommand.Contains("anade") ||
            normalizedCommand.Contains("pon") && normalizedCommand.Contains("en la lista") ||
            normalizedCommand.Contains("quiero agregar") ||
            normalizedCommand.Contains("necesito"))
        {
            var (producto, cantidad, unidad) = ExtractProductWithQuantity(normalizedCommand);
            return await HandleAgregarProductoFromText(producto, cantidad, unidad);
        }

        // AYUDA - HelpIntent
        if (normalizedCommand.Contains("ayuda") ||
            normalizedCommand.Contains("ayúdame") ||
            normalizedCommand.Contains("ayudame") ||
            normalizedCommand.Contains("qué puedo hacer") ||
            normalizedCommand.Contains("que puedo hacer") ||
            normalizedCommand.Contains("qué puedes hacer") ||
            normalizedCommand.Contains("que puedes hacer") ||
            normalizedCommand.Contains("cómo funciona") ||
            normalizedCommand.Contains("como funciona"))
        {
            return HandleHelpIntent();
        }

        // SALIR/CANCELAR - StopIntent/CancelIntent
        if (normalizedCommand.Contains("cancelar") ||
            normalizedCommand.Contains("olvídalo") ||
            normalizedCommand.Contains("olvidalo") ||
            normalizedCommand.Contains("déjalo") ||
            normalizedCommand.Contains("dejalo") ||
            normalizedCommand.Contains("detener") ||
            normalizedCommand.Contains("parar") ||
            normalizedCommand.Contains("terminar") ||
            normalizedCommand.Contains("salir") ||
            normalizedCommand.Contains("adiós") ||
            normalizedCommand.Contains("adios"))
        {
            return HandleStopIntent();
        }

        // Si no coincide con ningún patrón
        return HandleUnknownIntent();
    }

    private string ExtractListName(string command)
    {
        // Patrones para extraer nombre de lista al crear
        var patterns = new[]
        {
            "llamada ", "llamado ", "de nombre ", "nombre ",
            "lista ", "nueva lista ", "crear lista "
        };

        foreach (var pattern in patterns)
        {
            var index = command.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var nombre = command.Substring(index + pattern.Length).Trim();
                // Limpiar palabras finales comunes
                nombre = nombre
                    .Replace(" por favor", "")
                    .Replace(" gracias", "")
                    .Trim();

                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    return nombre;
                }
            }
        }

        // Si no se encuentra un patrón, retornar un nombre por defecto
        return "Mi Lista";
    }

    private string ExtractListNameToDelete(string command)
    {
        // Patrones para extraer nombre de lista al eliminar
        var patterns = new[]
        {
            "elimina la lista ", "borra la lista ", "quita la lista ",
            "eliminar lista ", "borrar la lista ", "borrar lista "
        };

        foreach (var pattern in patterns)
        {
            var index = command.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var nombre = command.Substring(index + pattern.Length).Trim();
                return nombre;
            }
        }

        return "";
    }

    private (string producto, decimal cantidad, string? unidad) ExtractProductWithQuantity(string command)
    {
        // Patrones para extraer producto con cantidad y unidad
        var patterns = new[]
        {
            "agrega ", "añade ", "anade ", "agregar ", "añadir ",
            "pon ", "quiero agregar ", "necesito "
        };

        foreach (var pattern in patterns)
        {
            var index = command.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var resto = command.Substring(index + pattern.Length).Trim();

                // Limpiar palabras finales
                resto = resto
                    .Replace(" en la lista", "")
                    .Replace(" a la lista", "")
                    .Replace(" por favor", "")
                    .Replace(" gracias", "")
                    .Trim();

                // Intentar extraer cantidad y unidad
                // Ejemplos: "2 manzanas", "3 kilos de tomates", "5 litros de agua", "1 kilo de carne"
                var parts = resto.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length > 0)
                {
                    // Verificar si el primer elemento es un número
                    if (decimal.TryParse(parts[0], out decimal cantidad))
                    {
                        if (parts.Length == 1)
                        {
                            return ("producto", cantidad, null);
                        }

                        // Verificar si hay unidad (kilo, litro, gramo, etc.)
                        var unidadesComunes = new[] { "kilo", "kilos", "litro", "litros", "gramo", "gramos", "unidad", "unidades" };
                        if (parts.Length > 1 && unidadesComunes.Any(u => parts[1].ToLower().Contains(u)))
                        {
                            var unidad = parts[1];

                            // Si sigue "de", tomar el resto como producto
                            if (parts.Length > 2 && parts[2].ToLower() == "de")
                            {
                                var producto = string.Join(" ", parts.Skip(3));
                                return (producto, cantidad, unidad);
                            }
                            else
                            {
                                var producto = string.Join(" ", parts.Skip(2));
                                return (producto, cantidad, unidad);
                            }
                        }
                        else
                        {
                            // Sin unidad específica, el resto es el producto
                            var producto = string.Join(" ", parts.Skip(1));
                            return (producto, cantidad, null);
                        }
                    }
                }

                // Si no hay cantidad, retornar el texto completo como producto
                return (resto, 1, null);
            }
        }

        return ("", 1, null);
    }

    private string ExtractProductNameForMark(string command)
    {
        // Patrones para extraer producto al marcar como comprado
        var patterns = new[]
        {
            "marca ", "marcar ", "compré ", "compre ", "ya compré ", "ya compre "
        };

        foreach (var pattern in patterns)
        {
            var index = command.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var producto = command.Substring(index + pattern.Length).Trim();
                // Limpiar palabras relacionadas con "comprado"
                producto = producto
                    .Replace(" como comprado", "")
                    .Replace(" como comprada", "")
                    .Replace(" comprado", "")
                    .Replace(" está comprado", "")
                    .Replace(" esta comprado", "")
                    .Replace(" a comprado", "")
                    .Replace(" ya está comprado", "")
                    .Replace(" ya esta comprado", "")
                    .Trim();
                return producto;
            }
        }

        // Patrones alternativos para otros casos
        if (command.Contains(" está comprado") || command.Contains(" esta comprado"))
        {
            var producto = command
                .Replace(" está comprado", "")
                .Replace(" esta comprado", "")
                .Replace("el ", "")
                .Replace("la ", "")
                .Trim();
            return producto;
        }

        return "";
    }

    private string ExtractProductNameToDelete(string command)
    {
        // Patrones para extraer producto al eliminar
        var patterns = new[]
        {
            "elimina ", "borra ", "quita ", "quiero quitar ",
            "eliminar ", "borrar "
        };

        foreach (var pattern in patterns)
        {
            var index = command.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var producto = command.Substring(index + pattern.Length).Trim();
                producto = producto
                    .Replace(" de la lista", "")
                    .Replace(" por favor", "")
                    .Replace(" gracias", "")
                    .Trim();
                return producto;
            }
        }

        return "";
    }

    private async Task<IActionResult> HandleCrearListaFromText(string nombreLista)
    {
        try
        {
            if (string.IsNullOrEmpty(nombreLista))
            {
                nombreLista = "Mi Lista";
            }

            var dto = new CrearListaCompraDto
            {
                Nombre = nombreLista,
                FechaObjetivo = DateTime.Today
            };

            var lista = await _listaService.CrearListaAsync(dto);

            var speechText = $"Perfecto, he creado la lista {nombreLista}. ¿Deseas agregar productos ahora?";

            return Ok(BuildAlexaResponse(speechText, repromptText: "¿Quieres agregar algún producto?"));
        }
        catch (InvalidOperationException ex)
        {
            return Ok(BuildAlexaResponse(ex.Message, shouldEndSession: false));
        }
    }

    private async Task<IActionResult> HandleAgregarProductoFromText(string producto, decimal cantidad, string? unidad)
    {
        try
        {
            if (string.IsNullOrEmpty(producto))
            {
                return Ok(BuildAlexaResponse(
                    "No escuché qué producto quieres agregar. ¿Qué producto necesitas?",
                    repromptText: "¿Qué producto quieres agregar?"
                ));
            }

            var listas = await _listaService.ObtenerListasPorFechaAsync(DateTime.Today);
            // Obtener la lista más reciente (última creada) ordenando por IdLista descendente
            var lista = listas.OrderByDescending(l => l.FechaCreacion).FirstOrDefault();

            if (lista == null)
            {
                return Ok(BuildAlexaResponse(
                    "No tienes ninguna lista activa para hoy. Primero crea una lista.",
                    repromptText: "¿Quieres crear una lista ahora?"
                ));
            }

            var dto = new AgregarProductoDto
            {
                IdLista = lista.IdLista,
                NombreProducto = producto,
                Cantidad = cantidad,
                Unidad = unidad
            };

            await _productoService.AgregarProductoAsync(dto);

            var cantidadTexto = cantidad > 1 ? $"{cantidad} " : "";
            var unidadTexto = !string.IsNullOrEmpty(unidad) ? $"{unidad} de " : "";

            var speechText = $"He agregado {cantidadTexto}{unidadTexto}{producto} a tu lista {lista.Nombre}. ¿Algo más?";

            return Ok(BuildAlexaResponse(speechText, repromptText: "¿Quieres agregar otro producto?"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar producto desde texto");
            return Ok(BuildAlexaResponse("Hubo un problema al agregar el producto. Intenta nuevamente."));
        }
    }

    private async Task<IActionResult> HandleEliminarListaFromText(string nombreLista)
    {
        try
        {
            if (string.IsNullOrEmpty(nombreLista))
            {
                return Ok(BuildAlexaResponse(
                    "¿Qué lista quieres eliminar?",
                    repromptText: "Dime el nombre de la lista que quieres eliminar"
                ));
            }

            var listas = await _listaService.ObtenerListasPorFechaAsync(DateTime.Today);
            var lista = listas.FirstOrDefault(l =>
                l.Nombre.Equals(nombreLista, StringComparison.OrdinalIgnoreCase));

            if (lista == null)
            {
                return Ok(BuildAlexaResponse(
                    $"No encontré ninguna lista llamada {nombreLista}.",
                    shouldEndSession: false
                ));
            }

            await _listaService.EliminarListaAsync(lista.IdLista);

            return Ok(BuildAlexaResponse(
                $"He eliminado la lista {nombreLista}.",
                shouldEndSession: false
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar lista desde texto");
            return Ok(BuildAlexaResponse("Hubo un problema al eliminar la lista. Intenta nuevamente."));
        }
    }

    private async Task<IActionResult> HandleEliminarProductoFromText(string producto)
    {
        try
        {
            if (string.IsNullOrEmpty(producto))
            {
                return Ok(BuildAlexaResponse(
                    "¿Qué producto quieres eliminar?",
                    repromptText: "Dime el nombre del producto que quieres eliminar"
                ));
            }

            var listas = await _listaService.ObtenerListasPorFechaAsync(DateTime.Today);

            foreach (var lista in listas)
            {
                var productos = await _productoService.ObtenerProductosPorListaAsync(lista.IdLista);
                var productoEncontrado = productos.FirstOrDefault(p =>
                    p.NombreProducto.ToLower().Contains(producto.ToLower()));

                if (productoEncontrado != null)
                {
                    await _productoService.EliminarProductoAsync(productoEncontrado.IdItem);

                    return Ok(BuildAlexaResponse(
                        $"He eliminado {producto} de tu lista {lista.Nombre}.",
                        shouldEndSession: false
                    ));
                }
            }

            return Ok(BuildAlexaResponse($"No encontré {producto} en tus listas."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar producto desde texto");
            return Ok(BuildAlexaResponse("Hubo un problema al eliminar el producto. Intenta nuevamente."));
        }
    }

    private async Task<IActionResult> HandleMarcarProductoFromText(string producto)
    {
        try
        {
            if (string.IsNullOrEmpty(producto))
            {
                return Ok(BuildAlexaResponse("¿Qué producto quieres marcar?"));
            }

            var listas = await _listaService.ObtenerListasPorFechaAsync(DateTime.Today);

            foreach (var lista in listas)
            {
                var productos = await _productoService.ObtenerProductosPorListaAsync(lista.IdLista);
                var productoEncontrado = productos.FirstOrDefault(p =>
                    p.NombreProducto.ToLower().Contains(producto.ToLower()));

                if (productoEncontrado != null)
                {
                    await _productoService.CambiarEstadoProductoAsync(new CambiarEstadoProductoDto
                    {
                        IdItem = productoEncontrado.IdItem,
                        NuevoEstado = BC.Enums.EstadoProducto.Comprado
                    });

                    return Ok(BuildAlexaResponse(
                        $"Perfecto, he marcado {producto} como comprado en tu lista {lista.Nombre}.",
                        shouldEndSession: false
                    ));
                }
            }

            return Ok(BuildAlexaResponse($"No encontré {producto} en tus listas."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar producto desde texto");
            return Ok(BuildAlexaResponse("Hubo un problema al marcar el producto."));
        }
    }

    private IActionResult HandleLaunchRequest()
    {
        _logger.LogInformation("Procesando LaunchRequest");
        
        var speechText = "¡Bienvenido a Lista de Compras! " +
                        "Puedes crear una lista, agregar productos, consultar tus listas o marcar productos como comprados. " +
                        "¿Qué te gustaría hacer?";

        return Ok(BuildAlexaResponse(speechText, repromptText: "¿Necesitas ayuda para comenzar?"));
    }

    private async Task<IActionResult> HandleIntentRequest(System.Text.Json.JsonElement request)
    {
        string intentName = "";
        if (request.TryGetProperty("request", out var requestObj) &&
            requestObj.TryGetProperty("intent", out var intentObj) &&
            intentObj.TryGetProperty("name", out var nameObj))
        {
            intentName = nameObj.GetString() ?? "";
        }

        _logger.LogInformation("Procesando intent: {IntentName}", intentName);

        return intentName switch
        {
            "CrearListaIntent" => await HandleCrearListaIntent(request),
            "ListarListasIntent" => await HandleListarListasIntent(request),
            "AgregarProductoIntent" => await HandleAgregarProductoIntent(request),
            "ListarProductosIntent" => await HandleListarProductosIntent(request),
            "MarcarProductoIntent" => await HandleMarcarProductoIntent(request),
            "AMAZON.HelpIntent" => HandleHelpIntent(),
            "AMAZON.CancelIntent" or "AMAZON.StopIntent" => HandleStopIntent(),
            _ => HandleUnknownIntent()
        };
    }
    
    private string GetSlotValue(System.Text.Json.JsonElement request, string slotName)
    {
        try
        {
            if (request.TryGetProperty("request", out var requestObj) &&
                requestObj.TryGetProperty("intent", out var intentObj) &&
                intentObj.TryGetProperty("slots", out var slotsObj) &&
                slotsObj.TryGetProperty(slotName, out var slotObj) &&
                slotObj.TryGetProperty("value", out var valueObj))
            {
                return valueObj.GetString() ?? "";
            }
        }
        catch { }
    
        return "";
    }

    private async Task<IActionResult> HandleCrearListaIntent(System.Text.Json.JsonElement request)
    {
        try
        {
            var nombreLista = GetSlotValue(request, "nombreLista");
        
            if (string.IsNullOrEmpty(nombreLista))
            {
                return Ok(BuildAlexaResponse(
                    "No escuché el nombre de la lista. ¿Cómo quieres llamar a tu lista?",
                    repromptText: "¿Qué nombre le ponemos a la lista?"
                ));
            }

            var dto = new CrearListaCompraDto
            {
                Nombre = nombreLista,
                FechaObjetivo = DateTime.Today
            };

            var lista = await _listaService.CrearListaAsync(dto);

            var speechText = $"Perfecto, he creado la lista {nombreLista}. ¿Deseas agregar productos ahora?";

            return Ok(BuildAlexaResponse(speechText, repromptText: "¿Quieres agregar algún producto?"));
        }
        catch (InvalidOperationException ex)
        {
            return Ok(BuildAlexaResponse(ex.Message, shouldEndSession: false));
        }
    }

    private async Task<IActionResult> HandleListarListasIntent(System.Text.Json.JsonElement request)
    {
        try
        {
            var listas = await _listaService.ObtenerListasPorFechaAsync(DateTime.Today);
            var listasList = listas.ToList();

            if (!listasList.Any())
            {
                return Ok(BuildAlexaResponse(
                    "No tienes listas para hoy. ¿Quieres crear una?",
                    repromptText: "¿Deseas crear una nueva lista?"
                ));
            }

            var nombres = string.Join(", ", listasList.Select(l => l.Nombre));
            var cantidad = listasList.Count;

            var speechText = cantidad == 1
                ? $"Tienes una lista para hoy: {nombres}"
                : $"Tienes {cantidad} listas para hoy: {nombres}";

            speechText += ". ¿Quieres ver los productos de alguna lista?";

            return Ok(BuildAlexaResponse(speechText, repromptText: "¿Qué lista quieres revisar?"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar listas");
            return Ok(BuildAlexaResponse("Hubo un problema al obtener tus listas. Intenta nuevamente."));
        }
    }

    private async Task<IActionResult> HandleAgregarProductoIntent(System.Text.Json.JsonElement request)
    {
        try
        {
            var producto = GetSlotValue(request, "producto");
            var cantidadStr = GetSlotValue(request, "cantidad");
            var unidad = GetSlotValue(request, "unidad");

            if (string.IsNullOrEmpty(producto))
            {
                return Ok(BuildAlexaResponse(
                    "No escuché qué producto quieres agregar. ¿Qué producto necesitas?",
                    repromptText: "¿Qué producto quieres agregar?"
                ));
            }

            // Obtener la lista más reciente (última creada)
            var listas = await _listaService.ObtenerListasPorFechaAsync(DateTime.Today);
            var lista = listas.OrderByDescending(l => l.FechaCreacion).FirstOrDefault();

            if (lista == null)
            {
                return Ok(BuildAlexaResponse(
                    "No tienes ninguna lista activa para hoy. Primero crea una lista diciendo: crea una lista.",
                    repromptText: "¿Quieres crear una lista ahora?"
                ));
            }

            var dto = new AgregarProductoDto
            {
                IdLista = lista.IdLista,
                NombreProducto = producto,
                Cantidad = decimal.TryParse((string)cantidadStr, out var cant) ? cant : 1,
                Unidad = unidad
            };

            await _productoService.AgregarProductoAsync(dto);

            var cantidadTexto = dto.Cantidad > 1 ? $"{dto.Cantidad} " : "";
            var unidadTexto = !string.IsNullOrEmpty(unidad) ? $"{unidad} de " : "";

            var speechText = $"He agregado {cantidadTexto}{unidadTexto}{producto} a tu lista {lista.Nombre}. ¿Algo más?";

            return Ok(BuildAlexaResponse(speechText, repromptText: "¿Quieres agregar otro producto?"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar producto");
            return Ok(BuildAlexaResponse("Hubo un problema al agregar el producto. Intenta nuevamente."));
        }
    }

    private async Task<IActionResult> HandleListarProductosIntent(System.Text.Json.JsonElement request)
    {
        try
        {
            // Obtener la lista más reciente (última creada)
            var listas = await _listaService.ObtenerListasPorFechaAsync(DateTime.Today);
            var lista = listas.OrderByDescending(l => l.FechaCreacion).FirstOrDefault();

            if (lista == null)
            {
                return Ok(BuildAlexaResponse(
                    "No tienes listas activas para hoy.",
                    shouldEndSession: false
                ));
            }

            var productos = await _productoService.ObtenerProductosPorListaAsync(lista.IdLista);
            var productosList = productos.ToList();

            if (!productosList.Any())
            {
                return Ok(BuildAlexaResponse(
                    $"Tu lista {lista.Nombre} está vacía. ¿Quieres agregar productos?",
                    repromptText: "¿Deseas agregar algún producto?"
                ));
            }

            var pendientes = productosList.Where(p => p.Estado == BC.Enums.EstadoProducto.Pendiente).ToList();
            var comprados = productosList.Where(p => p.Estado == BC.Enums.EstadoProducto.Comprado).ToList();

            var speechText = $"En tu lista {lista.Nombre} tienes: ";

            if (pendientes.Any())
            {
                var nombresPendientes = string.Join(", ", pendientes.Select(p =>
                    p.Cantidad > 1 ? $"{p.Cantidad} {p.Unidad ?? ""} de {p.NombreProducto}".Trim() : p.NombreProducto
                ));
                speechText += $"Pendientes: {nombresPendientes}. ";
            }

            if (comprados.Any())
            {
                speechText += $"Ya compraste {comprados.Count} producto{(comprados.Count > 1 ? "s" : "")}.";
            }

            return Ok(BuildAlexaResponse(speechText, shouldEndSession: false));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar productos");
            return Ok(BuildAlexaResponse("Hubo un problema al obtener los productos."));
        }
    }

    private async Task<IActionResult> HandleMarcarProductoIntent(System.Text.Json.JsonElement request)
    {
        try
        {
            var producto = GetSlotValue(request, "producto");

            if (string.IsNullOrEmpty(producto))
            {
                return Ok(BuildAlexaResponse("¿Qué producto quieres marcar?"));
            }

            // Buscar el producto en las listas del día
            var listas = await _listaService.ObtenerListasPorFechaAsync(DateTime.Today);

            foreach (var lista in listas)
            {
                var productos = await _productoService.ObtenerProductosPorListaAsync(lista.IdLista);
                var productoEncontrado = productos.FirstOrDefault(p =>
                    p.NombreProducto.ToLower().Contains(producto.ToLower()));

                if (productoEncontrado != null)
                {
                    await _productoService.CambiarEstadoProductoAsync(new CambiarEstadoProductoDto
                    {
                        IdItem = productoEncontrado.IdItem,
                        NuevoEstado = BC.Enums.EstadoProducto.Comprado
                    });

                    return Ok(BuildAlexaResponse(
                        $"Perfecto, he marcado {producto} como comprado en tu lista {lista.Nombre}.",
                        shouldEndSession: false
                    ));
                }
            }

            return Ok(BuildAlexaResponse($"No encontré {producto} en tus listas."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al marcar producto");
            return Ok(BuildAlexaResponse("Hubo un problema al marcar el producto."));
        }
    }

    private IActionResult HandleHelpIntent()
    {
        var speechText = "Con esta skill puedes crear listas de compras y agregar productos. " +
                        "Puedes decir: crea una lista llamada supermercado, " +
                        "agrega leche a la lista, " +
                        "qué hay en mi lista, " +
                        "o marca la leche como comprada. " +
                        "¿Qué te gustaría hacer?";

        return Ok(BuildAlexaResponse(speechText, repromptText: "¿Necesitas ayuda con algo específico?"));
    }

    private IActionResult HandleStopIntent()
    {
        return Ok(BuildAlexaResponse(
            "¡Hasta luego! Que tengas buenas compras.",
            shouldEndSession: true
        ));
    }

    private IActionResult HandleUnknownIntent()
    {
        return Ok(BuildAlexaResponse(
            "No entendí eso. Puedes pedirme crear una lista, agregar productos, o consultar tus listas.",
            repromptText: "¿Qué te gustaría hacer?"
        ));
    }

    private IActionResult HandleSessionEndedRequest()
    {
        _logger.LogInformation("Sesión terminada");
        return Ok();
    }

    /// <summary>
    /// Construye una respuesta en el formato esperado por Alexa
    /// </summary>
    private object BuildAlexaResponse(
        string speechText, 
        string? repromptText = null, 
        bool shouldEndSession = false)
    {
        var response = new
        {
            version = "1.0",
            response = new
            {
                outputSpeech = new
                {
                    type = "PlainText",
                    text = speechText
                },
                reprompt = repromptText != null ? new
                {
                    outputSpeech = new
                    {
                        type = "PlainText",
                        text = repromptText
                    }
                } : null,
                shouldEndSession
            }
        };

        return response;
    }
}