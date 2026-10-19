using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MediaGrab.Infrastructure;
using MediaGrab.Models;
using Xunit;

namespace MediaGrab.Tests;

public class ProcessRunnerTests
{
    [Fact]
    public async Task ComandoInexistente_DevuelveErrorSinLanzarExcepcion()
    {
        ProcessResult result = await ProcessRunner.RunAsync("comando-que-no-existe-mediagrab", []);

        Assert.Equal(ProcessRunner.StartFailedExitCode, result.ExitCode);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task CapturaLaSalidaYElCodigoDeSalida()
    {
        ProcessResult result = await ProcessRunner.RunAsync("cmd.exe", ["/c", "echo hola& exit 3"]);

        Assert.Equal(3, result.ExitCode);
        Assert.Equal("hola", result.FirstLine);
    }

    [Fact]
    public async Task Cancelar_TerminaElProcesoYSusHijos()
    {
        using CancellationTokenSource cancellation = new(TimeSpan.FromSeconds(1));
        Stopwatch elapsed = Stopwatch.StartNew();

        // cmd lanza ping como proceso hijo, igual que yt-dlp lanza ffmpeg.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => ProcessRunner.RunAsync("cmd.exe", ["/c", "ping -n 30 127.0.0.1 > nul"], cancellationToken: cancellation.Token));

        Assert.True(elapsed.Elapsed < TimeSpan.FromSeconds(10), "La cancelación debería ser inmediata.");
    }

    [Fact]
    public void YtDlpCommand_MuestraLaRutaEntreComillasSiTieneEspacios()
    {
        YtDlpCommand command = new("C:\\Program Files\\yt-dlp.exe");

        Assert.Equal("\"C:\\Program Files\\yt-dlp.exe\"", command.ToString());
        Assert.False(command.IsPythonModule);
    }

    [Fact]
    public void YtDlpCommand_ComoModuloDePython()
    {
        YtDlpCommand command = new("py", "-m", "yt_dlp");

        Assert.True(command.IsPythonModule);
        Assert.Equal(["-m", "yt_dlp", "--version"], command.WithArguments("--version"));
    }
}
