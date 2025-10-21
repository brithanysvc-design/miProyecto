namespace SkillAlexa.API.Models;

public class ApiResponse<T>
{
    /// <summary>
    /// Indica si la operación fue exitosa
    /// </summary>
    public bool Exitoso { get; set; }

    /// <summary>
    /// Mensaje descriptivo de la respuesta
    /// </summary>
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Datos de la respuesta
    /// </summary>
    public T? Datos { get; set; }

    /// <summary>
    /// Lista de errores de validación (si los hay)
    /// </summary>
    public List<string>? Errores { get; set; }

    /// <summary>
    /// Crea una respuesta exitosa
    /// </summary>
    public static ApiResponse<T> Success(T datos, string mensaje = "Operación exitosa")
    {
        return new ApiResponse<T>
        {
            Exitoso = true,
            Mensaje = mensaje,
            Datos = datos
        };
    }

    /// <summary>
    /// Crea una respuesta de error
    /// </summary>
    public static ApiResponse<T> Error(string mensaje, List<string>? errores = null)
    {
        return new ApiResponse<T>
        {
            Exitoso = false,
            Mensaje = mensaje,
            Errores = errores
        };
    }
}