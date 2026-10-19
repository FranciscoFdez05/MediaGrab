using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaGrab.Infrastructure;

/// <summary>
/// Resultado de ejecutar un proceso externo.
/// </summary>
internal sealed class ProcessResult(int exitCode, string output)
{
    public int ExitCode { get; } = exitCode;

    /// <summary>Salida estándar y de error combinadas, en el orden en que llegaron.</summary>
    public string Output { get; } = output ?? string.Empty;

    public bool Succeeded => ExitCode == 0;

    public string FirstLine =>
        Output.Split(["\r\n", "\n"], StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.Trim())
            .FirstOrDefault() ?? string.Empty;
}

/// <summary>
/// Ejecuta procesos externos sin ventana, leyendo stdout y stderr de forma asíncrona
/// (evita bloqueos cuando uno de los dos buffers se llena) y con soporte de cancelación.
/// </summary>
internal static class ProcessRunner
{
    /// <summary>Código de salida usado cuando el proceso ni siquiera pudo arrancar.</summary>
    public const int StartFailedExitCode = -1;

    public static async Task<ProcessResult> RunAsync(
        string fileName,
        IEnumerable<string> arguments,
        Action<string> onLine = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        StringBuilder output = new();
        Lock outputLock = new();

        void OnData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data is null)
            {
                return;
            }

            lock (outputLock)
            {
                output.AppendLine(e.Data);
            }

            onLine?.Invoke(e.Data);
        }

        using Process process = new() { StartInfo = CreateStartInfo(fileName, arguments) };
        process.OutputDataReceived += OnData;
        process.ErrorDataReceived += OnData;

        try
        {
            process.Start();
        }
        catch (Win32Exception ex)
        {
            // El ejecutable no existe o no se puede ejecutar.
            return new ProcessResult(StartFailedExitCode, ex.Message);
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            // También espera a que se vacíen stdout y stderr.
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // yt-dlp lanza ffmpeg como hijo: se termina el árbol completo para no dejar procesos huérfanos.
            TryKill(process);
            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            throw;
        }

        lock (outputLock)
        {
            return new ProcessResult(process.ExitCode, output.ToString().Trim());
        }
    }

    /// <summary>
    /// Devuelve true si el comando existe y responde correctamente a los argumentos dados.
    /// </summary>
    public static async Task<bool> SucceedsAsync(string fileName, params string[] arguments)
    {
        ProcessResult result = await RunAsync(fileName, arguments).ConfigureAwait(false);
        return result.Succeeded;
    }

    private static ProcessStartInfo CreateStartInfo(string fileName, IEnumerable<string> arguments)
    {
        ProcessStartInfo startInfo = new(fileName)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        // ArgumentList escapa cada argumento según las reglas de Windows (comillas, barras...).
        foreach (string argument in arguments ?? [])
        {
            startInfo.ArgumentList.Add(argument);
        }

        // Fuerza a Python (yt-dlp, pip) a escribir en UTF-8 aunque la salida esté redirigida,
        // para que títulos con acentos o emojis no lleguen corruptos.
        startInfo.Environment["PYTHONIOENCODING"] = "utf-8";
        startInfo.Environment["PYTHONUTF8"] = "1";

        return startInfo;
    }

    private static void TryKill(Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (Exception ex) when (ex is InvalidOperationException or Win32Exception or AggregateException)
        {
            // El proceso ya había terminado o no se pudo terminar alguno de sus hijos.
        }
    }
}
