using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MediaGrab.Infrastructure;
using MediaGrab.Models;

namespace MediaGrab.Services
{
    /// <summary>
    /// Localiza yt-dlp en el equipo. El resultado se guarda en caché porque cada comprobación
    /// lanza varios procesos; tras instalar o actualizar hay que pedir una búsqueda nueva.
    /// </summary>
    internal sealed class YtDlpLocator
    {
        private const string ExecutableName = "yt-dlp.exe";
        private static readonly string[] PythonLaunchers = { "python", "py" };

        private YtDlpCommand cached;

        public async Task<YtDlpCommand> FindAsync(bool refresh = false)
        {
            if (refresh || cached == null)
            {
                cached = await SearchAsync().ConfigureAwait(false);
            }

            return cached;
        }

        /// <summary>Devuelve el primer lanzador de Python disponible ("python" o "py"), o null.</summary>
        public static async Task<string> FindPythonAsync()
        {
            foreach (string launcher in PythonLaunchers)
            {
                if (await ProcessRunner.SucceedsAsync(launcher, "--version").ConfigureAwait(false))
                {
                    return launcher;
                }
            }

            return null;
        }

        private static async Task<YtDlpCommand> SearchAsync()
        {
            if (await ProcessRunner.SucceedsAsync("yt-dlp", "--version").ConfigureAwait(false))
            {
                return new YtDlpCommand("yt-dlp");
            }

            // El PATH de este proceso no se actualiza tras instalar con winget o pip,
            // así que se buscan también las rutas de instalación habituales.
            string knownPath = KnownInstallPaths().FirstOrDefault(File.Exists);
            if (knownPath != null)
            {
                return new YtDlpCommand(knownPath);
            }

            foreach (string launcher in PythonLaunchers)
            {
                if (await ProcessRunner.SucceedsAsync(launcher, "-c", "import yt_dlp").ConfigureAwait(false))
                {
                    return new YtDlpCommand(launcher, "-m", "yt_dlp");
                }
            }

            return null;
        }

        private static IEnumerable<string> KnownInstallPaths()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string roamingAppData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

            yield return Path.Combine(localAppData, "Microsoft", "WinGet", "Links", ExecutableName);

            foreach (string package in SafeGetDirectories(Path.Combine(localAppData, "Microsoft", "WinGet", "Packages"), "yt-dlp.yt-dlp_*"))
            {
                yield return Path.Combine(package, ExecutableName);
            }

            // Instalaciones con pip (global y --user), de la versión de Python más reciente a la más antigua.
            foreach (string pythonRoot in new[]
            {
                Path.Combine(localAppData, "Programs", "Python"),
                Path.Combine(roamingAppData, "Python")
            })
            {
                foreach (string python in SafeGetDirectories(pythonRoot, "Python3*").OrderByDescending(d => d, StringComparer.OrdinalIgnoreCase))
                {
                    yield return Path.Combine(python, "Scripts", ExecutableName);
                }
            }
        }

        private static IEnumerable<string> SafeGetDirectories(string root, string pattern)
        {
            try
            {
                return Directory.Exists(root) ? Directory.GetDirectories(root, pattern) : new string[0];
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                return new string[0];
            }
        }
    }
}
