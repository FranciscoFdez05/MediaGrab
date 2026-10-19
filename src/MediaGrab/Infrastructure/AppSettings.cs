using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using MediaGrab.Models;

namespace MediaGrab.Infrastructure;

/// <summary>
/// Preferencias del usuario, guardadas en %APPDATA%\MediaGrab\settings.json.
/// </summary>
internal sealed class AppSettings
{
    private static readonly string FilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MediaGrab", "settings.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public string DownloadFolder { get; set; }

    public DownloadFormat DownloadFormat { get; set; } = DownloadFormat.Mp4;

    /// <summary>Altura máxima del vídeo (0 = la mejor).</summary>
    public int VideoQuality { get; set; }

    /// <summary>Bitrate del audio en kbps (0 = el mejor).</summary>
    public int AudioQuality { get; set; }

    /// <summary>
    /// Carga las preferencias. Si el archivo no existe o está dañado, devuelve los valores por defecto.
    /// </summary>
    public static AppSettings Load()
    {
        try
        {
            return File.Exists(FilePath)
                ? JsonSerializer.Deserialize<AppSettings>(File.ReadAllText(FilePath), JsonOptions) ?? new AppSettings()
                : new AppSettings();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException)
        {
            return new AppSettings();
        }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath));
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this, JsonOptions));
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            // Perder las preferencias no debe impedir usar la aplicación.
        }
    }
}
