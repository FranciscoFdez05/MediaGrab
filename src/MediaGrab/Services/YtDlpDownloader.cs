using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaGrab.Infrastructure;
using MediaGrab.Models;

namespace MediaGrab.Services;

/// <summary>
/// Descarga vídeos o audio con yt-dlp, notificando el progreso y admitiendo cancelación.
/// </summary>
internal sealed class YtDlpDownloader
{
    private const string OutputTemplate = "%(title)s.%(ext)s";
    private const int MaxErrorSummaryLines = 15;

    // Aviso que yt-dlp muestra cuando lleva mucho sin actualizarse; suele explicar los errores 403.
    private const string OutdatedWarning = "is older than 90 days";

    public async Task<DownloadResult> DownloadAsync(
        YtDlpCommand command,
        DownloadRequest request,
        IProgress<DownloadProgress> progress,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(request.DestinationFolder);

        // yt-dlp escribe aquí la ruta final del archivo (tras unir o convertir) para poder mostrarlo.
        string filePathOutput = Path.GetTempFileName();

        try
        {
            DownloadProgressTracker tracker = new DownloadProgressTracker(request);
            object trackerLock = new object();

            // stdout y stderr llegan por hilos distintos; el tracker se serializa con un candado.
            Action<string> onLine = line =>
            {
                DownloadProgress? update;

                lock (trackerLock)
                {
                    update = tracker.Process(line);
                }

                if (update.HasValue)
                {
                    progress?.Report(update.Value);
                }
            };

            ProcessResult result = await ProcessRunner.RunAsync(
                command.Executable,
                command.WithArguments(BuildArguments(request, filePathOutput)),
                onLine,
                cancellationToken).ConfigureAwait(false);

            if (!result.Succeeded)
            {
                return DownloadResult.Failed(SummarizeErrors(result));
            }

            progress?.Report(new DownloadProgress(100, "Descarga completada."));
            return DownloadResult.Completed(ReadFinalPath(filePathOutput));
        }
        finally
        {
            TryDelete(filePathOutput);
        }
    }

    private static string[] BuildArguments(DownloadRequest request, string filePathOutput)
    {
        List<string> arguments = new List<string> { "--newline" };

        if (request.Format == DownloadFormat.Mp3)
        {
            arguments.AddRange(new[]
            {
                "--extract-audio", "--audio-format", "mp3",
                // 0 = la mejor calidad VBR; si no, bitrate fijo (p.ej. "192K").
                "--audio-quality", request.Quality > 0 ? request.Quality.ToString(CultureInfo.InvariantCulture) + "K" : "0"
            });
        }
        else
        {
            arguments.AddRange(new[] { "--format", VideoFormatSelector(request.Quality), "--merge-output-format", "mp4" });
        }

        arguments.AddRange(new[]
        {
            "--print-to-file", "after_move:filepath", filePathOutput,
            "--output", Path.Combine(request.DestinationFolder, OutputTemplate),
            // "--" impide que una URL que empiece por "-" se interprete como una opción.
            "--", request.Url
        });

        return arguments.ToArray();
    }

    private static string VideoFormatSelector(int maxHeight)
    {
        if (maxHeight <= 0)
        {
            return "bestvideo+bestaudio/best";
        }

        // El "/best" final cubre los sitios que no informan de la resolución (p.ej. enlaces directos).
        string limit = $"[height<={maxHeight.ToString(CultureInfo.InvariantCulture)}]";
        return $"bestvideo{limit}+bestaudio/best{limit}/best";
    }

    private static string ReadFinalPath(string filePathOutput)
    {
        try
        {
            // Con listas de reproducción hay una línea por archivo; se muestra el último.
            string path = File.ReadAllLines(filePathOutput, Encoding.UTF8)
                .Select(line => line.Trim())
                .LastOrDefault(line => line.Length > 0);

            return path != null && File.Exists(path) ? path : null;
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            return null;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
        {
            // Es un archivo temporal: si no se puede borrar ahora, lo hará Windows.
        }
    }

    /// <summary>
    /// Extrae las líneas "ERROR:" de la salida de yt-dlp o, si no hay, las últimas líneas.
    /// </summary>
    private static string SummarizeErrors(ProcessResult result)
    {
        string[] lines = result.Output
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .Where(line => line.Length > 0)
            .ToArray();

        string[] errors = lines
            .Where(line => line.StartsWith("ERROR:", StringComparison.OrdinalIgnoreCase))
            .ToArray();

        IEnumerable<string> summary = errors.Length > 0 ? errors : lines.Skip(Math.Max(0, lines.Length - MaxErrorSummaryLines));
        string text = string.Join(Environment.NewLine, summary);

        if (string.IsNullOrWhiteSpace(text))
        {
            text = $"No se pudo descargar el vídeo (código de salida {result.ExitCode}).";
        }

        if (result.Output.IndexOf(OutdatedWarning, StringComparison.OrdinalIgnoreCase) >= 0)
        {
            text += Environment.NewLine + Environment.NewLine +
                    "Tu versión de yt-dlp está desactualizada y es la causa más probable del error. " +
                    "Actualízala desde Herramientas > Actualizar yt-dlp.";
        }

        return text;
    }
}
