namespace MediaGrab.Models;

internal enum DownloadFormat
{
    Mp4,
    Mp3
}

internal sealed class DownloadRequest
{
    public DownloadRequest(string url, string destinationFolder, DownloadFormat format, int quality)
    {
        Url = url;
        DestinationFolder = destinationFolder;
        Format = format;
        Quality = quality;
    }

    public string Url { get; }

    public string DestinationFolder { get; }

    public DownloadFormat Format { get; }

    /// <summary>
    /// Altura máxima del vídeo en píxeles (MP4) o bitrate en kbps (MP3). 0 significa la mejor disponible.
    /// </summary>
    public int Quality { get; }

    public string FormatName => Format == DownloadFormat.Mp3 ? "MP3" : "MP4";
}

/// <summary>
/// Estado de la descarga notificado a la interfaz.
/// </summary>
internal struct DownloadProgress
{
    public DownloadProgress(int percent, string status, string title = null)
    {
        Percent = percent;
        Status = status;
        Title = title;
    }

    /// <summary>Porcentaje global (0-100), o -1 si aún no se conoce.</summary>
    public int Percent { get; }

    public string Status { get; }

    /// <summary>Título del archivo que se está descargando, o null si aún no se conoce.</summary>
    public string Title { get; }
}

internal sealed class DownloadResult
{
    private DownloadResult(bool success, string filePath, string message)
    {
        Success = success;
        FilePath = filePath;
        Message = message ?? string.Empty;
    }

    public bool Success { get; }

    /// <summary>Ruta del archivo final, o null si no se conoce.</summary>
    public string FilePath { get; }

    /// <summary>Resumen de errores de yt-dlp cuando la descarga falla.</summary>
    public string Message { get; }

    public static DownloadResult Completed(string filePath) => new DownloadResult(true, filePath, null);

    public static DownloadResult Failed(string message) => new DownloadResult(false, null, message);
}
