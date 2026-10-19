using System;
using System.Threading.Tasks;
using MediaGrab.Infrastructure;
using MediaGrab.Models;

namespace MediaGrab.Services;

/// <summary>
/// Instala y actualiza yt-dlp probando, por orden, el autoactualizador, winget y pip.
/// </summary>
internal sealed class YtDlpInstaller
{
    private const string WingetPackageId = "yt-dlp.yt-dlp";
    private const string UnknownVersion = "desconocida";

    private readonly YtDlpLocator locator;

    public YtDlpInstaller(YtDlpLocator locator)
    {
        this.locator = locator;
    }

    public static async Task<string> GetVersionAsync(YtDlpCommand command)
    {
        ProcessResult result = await ProcessRunner.RunAsync(command.Executable, command.WithArguments("--version")).ConfigureAwait(false);

        return result.Succeeded && result.FirstLine.Length > 0 ? result.FirstLine : UnknownVersion;
    }

    public async Task<OperationResult> InstallAsync()
    {
        OperationResult winget = await InstallWithWingetAsync().ConfigureAwait(false);
        if (winget.Success)
        {
            return winget;
        }

        OperationResult pip = await RunPipAsync(null).ConfigureAwait(false);
        if (pip.Success && await locator.FindAsync(refresh: true).ConfigureAwait(false) != null)
        {
            return OperationResult.Ok("yt-dlp instalado correctamente con pip.");
        }

        return OperationResult.Fail(
            $"Intento con winget:\r\n{winget.Message}\r\n\r\nIntento con pip:\r\n{pip.Message}");
    }

    public async Task<OperationResult> UpdateAsync(YtDlpCommand command)
    {
        // Un yt-dlp instalado como módulo de Python solo se actualiza con pip.
        if (command.IsPythonModule)
        {
            return await RunPipAsync(command.Executable).ConfigureAwait(false);
        }

        ProcessResult selfUpdate = await ProcessRunner.RunAsync(command.Executable, command.WithArguments("-U")).ConfigureAwait(false);
        if (selfUpdate.Succeeded)
        {
            return OperationResult.Ok(OperationResult.OrDefault(selfUpdate.Output, "yt-dlp actualizado con -U."));
        }

        // El autoactualizador se niega si yt-dlp vino de un gestor de paquetes.
        OperationResult winget = await UpgradeWithWingetAsync().ConfigureAwait(false);
        if (winget.Success)
        {
            return winget;
        }

        OperationResult pip = await RunPipAsync(null).ConfigureAwait(false);
        if (pip.Success)
        {
            return pip;
        }

        return OperationResult.Fail(
            $"Intento con -U:\r\n{selfUpdate.Output}\r\n\r\nIntento con winget:\r\n{winget.Message}\r\n\r\nIntento con pip:\r\n{pip.Message}");
    }

    private async Task<OperationResult> InstallWithWingetAsync()
    {
        ProcessResult result = await RunWingetAsync("install").ConfigureAwait(false);
        if (result == null)
        {
            return OperationResult.Fail("No se encontró winget en este equipo.");
        }

        // winget devuelve error si el paquete ya estaba instalado; lo que importa es si ahora se encuentra.
        if (await locator.FindAsync(refresh: true).ConfigureAwait(false) != null)
        {
            return OperationResult.Ok("yt-dlp instalado correctamente con winget.");
        }

        return OperationResult.Fail(
            OperationResult.OrDefault(result.Output, "Winget terminó, pero yt-dlp sigue sin estar disponible."));
    }

    private static async Task<OperationResult> UpgradeWithWingetAsync()
    {
        ProcessResult result = await RunWingetAsync("upgrade").ConfigureAwait(false);
        if (result == null)
        {
            return OperationResult.Fail("No se encontró winget en este equipo.");
        }

        if (ContainsAny(result.Output, "no available upgrade found", "no newer package versions are available"))
        {
            return OperationResult.Ok("yt-dlp ya estaba en la última versión disponible en winget.");
        }

        return result.Succeeded
            ? OperationResult.Ok(OperationResult.OrDefault(result.Output, "yt-dlp actualizado con winget."))
            : OperationResult.Fail(OperationResult.OrDefault(result.Output, "Winget no pudo actualizar yt-dlp."));
    }

    /// <summary>Ejecuta "winget install|upgrade" para yt-dlp, o devuelve null si winget no está disponible.</summary>
    private static async Task<ProcessResult> RunWingetAsync(string verb)
    {
        if (!await ProcessRunner.SucceedsAsync("winget", "--version").ConfigureAwait(false))
        {
            return null;
        }

        // Sin aceptar los acuerdos por argumento, winget se quedaría esperando una respuesta
        // por consola que nunca llega porque la entrada no es interactiva.
        return await ProcessRunner.RunAsync("winget", new[]
        {
            verb, "--id", WingetPackageId, "--exact",
            "--accept-source-agreements", "--accept-package-agreements"
        }).ConfigureAwait(false);
    }

    private static async Task<OperationResult> RunPipAsync(string python)
    {
        python = python ?? await YtDlpLocator.FindPythonAsync().ConfigureAwait(false);
        if (python == null)
        {
            return OperationResult.Fail("No se pudo ejecutar pip porque no hay Python disponible en el sistema.");
        }

        ProcessResult result = await ProcessRunner.RunAsync(python, new[] { "-m", "pip", "install", "--upgrade", "yt-dlp" }).ConfigureAwait(false);

        return result.Succeeded
            ? OperationResult.Ok(OperationResult.OrDefault(result.Output, "yt-dlp actualizado con pip."))
            : OperationResult.Fail(OperationResult.OrDefault(result.Output, "Pip no pudo instalar yt-dlp."));
    }

    private static bool ContainsAny(string text, params string[] values)
    {
        foreach (string value in values)
        {
            if (text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }
        }

        return false;
    }
}
