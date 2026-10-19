using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using MediaGrab.Models;

namespace MediaGrab.Services;

/// <summary>
/// Interpreta la salida de yt-dlp (ejecutado con --newline) y la convierte en un progreso global.
/// </summary>
/// <remarks>
/// Una descarga MP4 puede bajar vídeo y audio por separado antes de unirlos, así que se reparte
/// en varias etapas; cada línea "[download] Destination:" marca el inicio de una nueva.
/// No es seguro para hilos: quien lo use debe serializar las llamadas a <see cref="Process"/>.
/// </remarks>
internal sealed class DownloadProgressTracker
{
    // p.ej. "[download]  42.3% of ~ 12.34MiB at  1.23MiB/s ETA 00:10 (frag 3/20)"
    private static readonly Regex ProgressLine = new Regex(
        @"^\[download\]\s+(?<percent>\d{1,3}(?:[.,]\d+)?)%" +
        @"(?:.*?\bat\s+(?<speed>\S+))?" +
        @"(?:.*?\bETA\s+(?<eta>\S+))?",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // p.ej. "[info] aqz-KE-bpKQ: Downloading 1 format(s): 401+258" (vídeo y audio por separado).
    private static readonly Regex FormatsLine = new Regex(
        @"^\[info\].*\bDownloading \d+ format\(s\):\s*(?<formats>\S+)",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    // Sufijo de formato que yt-dlp añade a los archivos intermedios, p.ej. "Título.f401.mp4".
    private static readonly Regex FormatSuffix = new Regex(@"\.f[\w-]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private const string DestinationPrefix = "[download] Destination:";

    private readonly string formatName;
    private int currentStage;
    private int totalStages;
    private int percent = -1;
    private string status;
    private string title;

    public DownloadProgressTracker(DownloadRequest request)
    {
        formatName = request.FormatName;
        totalStages = request.Format == DownloadFormat.Mp3 ? 1 : 2;
    }

    /// <summary>
    /// Procesa una línea de salida. Devuelve el nuevo estado si ha cambiado, o null en caso contrario.
    /// </summary>
    public DownloadProgress? Process(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return null;
        }

        line = line.Trim();

        // yt-dlp anuncia cuántos archivos va a bajar: sustituye a la estimación inicial
        // (muchos sitios sirven vídeo y audio en un solo archivo aunque se pida MP4).
        Match formats = FormatsLine.Match(line);
        if (formats.Success)
        {
            totalStages = currentStage + formats.Groups["formats"].Value.Split('+').Length;
            return Update(percent, "Obteniendo información del vídeo...");
        }

        if (line.StartsWith(DestinationPrefix, StringComparison.OrdinalIgnoreCase))
        {
            currentStage++;
            totalStages = Math.Max(totalStages, currentStage);
            title = TitleFromPath(line.Substring(DestinationPrefix.Length).Trim()) ?? title;
            return Update(percent, "Descargando...");
        }

        if (line.StartsWith("[download]", StringComparison.OrdinalIgnoreCase) &&
            line.EndsWith("has already been downloaded", StringComparison.OrdinalIgnoreCase))
        {
            return Update(100, "El archivo ya estaba descargado.");
        }

        Match match = ProgressLine.Match(line);
        if (match.Success)
        {
            return HandleProgressLine(match);
        }

        if (line.StartsWith("[Merger]", StringComparison.OrdinalIgnoreCase))
        {
            return Update(percent, "Uniendo vídeo y audio...");
        }

        if (line.StartsWith("[ExtractAudio]", StringComparison.OrdinalIgnoreCase))
        {
            return Update(percent, $"Convirtiendo a {formatName}...");
        }

        if (line.StartsWith("[Fixup", StringComparison.OrdinalIgnoreCase))
        {
            return Update(percent, "Procesando archivo...");
        }

        // Antes de la primera descarga, yt-dlp está consultando la página del vídeo.
        if (currentStage == 0 && line.StartsWith("[", StringComparison.Ordinal))
        {
            return Update(percent, "Obteniendo información del vídeo...");
        }

        return null;
    }

    private DownloadProgress? HandleProgressLine(Match match)
    {
        if (!double.TryParse(
                match.Groups["percent"].Value.Replace(',', '.'),
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out double stagePercent))
        {
            return null;
        }

        // Sin línea de destino previa (por ejemplo al reanudar) se cuenta igualmente la primera etapa.
        int stage = Math.Max(currentStage, 1);
        int total = Math.Max(totalStages, stage);
        int overall = (int)Math.Round(((stage - 1) * 100.0 + Math.Min(stagePercent, 100.0)) / total);

        return Update(overall, DescribeTransfer(overall, match.Groups["speed"].Value, match.Groups["eta"].Value));
    }

    private static string DescribeTransfer(int overall, string speed, string eta)
    {
        // Texto corto: la etiqueta de estado es estrecha y la barra ya indica que se descarga.
        StringBuilder builder = new StringBuilder($"{overall}%");

        if (IsKnown(speed))
        {
            builder.Append(" · ").Append(speed);
        }

        if (IsKnown(eta))
        {
            builder.Append(" · quedan ").Append(eta);
        }

        return builder.ToString();
    }

    private static bool IsKnown(string value)
    {
        return !string.IsNullOrEmpty(value) && !value.StartsWith("Unknown", StringComparison.OrdinalIgnoreCase);
    }

    private DownloadProgress? Update(int newPercent, string newStatus)
    {
        // La barra nunca retrocede aunque el porcentaje de una etapa nueva empiece de cero.
        newPercent = Math.Max(percent, Math.Min(100, newPercent));

        if (newPercent == percent && newStatus == status)
        {
            return null;
        }

        percent = newPercent;
        status = newStatus;

        return new DownloadProgress(percent, status, title);
    }

    private static string TitleFromPath(string path)
    {
        try
        {
            string name = Path.GetFileNameWithoutExtension(path);
            return string.IsNullOrWhiteSpace(name) ? null : FormatSuffix.Replace(name, string.Empty);
        }
        catch (ArgumentException)
        {
            // Ruta con caracteres no válidos: no es grave, simplemente no se muestra el título.
            return null;
        }
    }
}
