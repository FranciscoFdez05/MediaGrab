namespace MediaGrab.Models;

/// <summary>
/// Resultado de una operación de mantenimiento (instalar o actualizar yt-dlp).
/// </summary>
internal sealed class OperationResult
{
    private OperationResult(bool success, string message)
    {
        Success = success;
        Message = message ?? string.Empty;
    }

    public bool Success { get; }

    public string Message { get; }

    public static OperationResult Ok(string message) => new OperationResult(true, message);

    public static OperationResult Fail(string message) => new OperationResult(false, message);

    /// <summary>Devuelve <paramref name="output"/> si tiene contenido o <paramref name="fallback"/> en caso contrario.</summary>
    public static string OrDefault(string output, string fallback)
    {
        return string.IsNullOrWhiteSpace(output) ? fallback : output;
    }
}
