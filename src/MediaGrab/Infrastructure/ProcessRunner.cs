using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaGrab.Infrastructure
{
    /// <summary>
    /// Resultado de ejecutar un proceso externo.
    /// </summary>
    internal sealed class ProcessResult
    {
        public ProcessResult(int exitCode, string output)
        {
            ExitCode = exitCode;
            Output = output ?? string.Empty;
        }

        public int ExitCode { get; }

        /// <summary>Salida estándar y de error combinadas, en el orden en que llegaron.</summary>
        public string Output { get; }

        public bool Succeeded => ExitCode == 0;

        public string FirstLine =>
            Output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(linea => linea.Trim())
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

        private const int CancellationPollMilliseconds = 200;

        public static Task<ProcessResult> RunAsync(
            string fileName,
            IEnumerable<string> arguments,
            Action<string> onLine = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() => Run(fileName, arguments, onLine, cancellationToken), cancellationToken);
        }

        /// <summary>
        /// Devuelve true si el comando existe y responde correctamente a los argumentos dados.
        /// </summary>
        public static async Task<bool> SucceedsAsync(string fileName, params string[] arguments)
        {
            ProcessResult result = await RunAsync(fileName, arguments).ConfigureAwait(false);
            return result.Succeeded;
        }

        private static ProcessResult Run(
            string fileName,
            IEnumerable<string> arguments,
            Action<string> onLine,
            CancellationToken cancellationToken)
        {
            StringBuilder output = new StringBuilder();
            object outputLock = new object();

            DataReceivedEventHandler handler = (sender, e) =>
            {
                if (e.Data == null)
                {
                    return;
                }

                lock (outputLock)
                {
                    output.AppendLine(e.Data);
                }

                onLine?.Invoke(e.Data);
            };

            using (Process process = new Process())
            {
                process.StartInfo = CreateStartInfo(fileName, arguments);
                process.OutputDataReceived += handler;
                process.ErrorDataReceived += handler;

                try
                {
                    process.Start();
                }
                catch (Exception ex)
                {
                    // Normalmente Win32Exception: el ejecutable no existe.
                    return new ProcessResult(StartFailedExitCode, ex.Message);
                }

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!process.WaitForExit(CancellationPollMilliseconds))
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        KillProcessTree(process);
                        process.WaitForExit();
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }

                // Sin timeout garantiza que los eventos de salida pendientes se han vaciado.
                process.WaitForExit();

                lock (outputLock)
                {
                    return new ProcessResult(process.ExitCode, output.ToString().Trim());
                }
            }
        }

        private static ProcessStartInfo CreateStartInfo(string fileName, IEnumerable<string> arguments)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = CommandLine.Join(arguments ?? Enumerable.Empty<string>()),
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            // Fuerza a Python (yt-dlp, pip) a escribir en UTF-8 aunque la salida esté redirigida,
            // para que títulos con acentos o emojis no lleguen corruptos.
            startInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";
            startInfo.EnvironmentVariables["PYTHONUTF8"] = "1";

            return startInfo;
        }

        private static void KillProcessTree(Process process)
        {
            // .NET Framework no tiene Kill(entireProcessTree); yt-dlp lanza ffmpeg como hijo,
            // así que hay que terminar el árbol completo para no dejar procesos huérfanos.
            try
            {
                using (Process taskkill = Process.Start(new ProcessStartInfo
                {
                    FileName = "taskkill.exe",
                    Arguments = $"/PID {process.Id} /T /F",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }))
                {
                    taskkill?.WaitForExit(5000);
                }
            }
            catch
            {
                // Si taskkill falla, al menos intentamos terminar el proceso principal.
            }

            try
            {
                if (!process.HasExited)
                {
                    process.Kill();
                }
            }
            catch (InvalidOperationException)
            {
                // El proceso ya terminó.
            }
        }
    }
}
