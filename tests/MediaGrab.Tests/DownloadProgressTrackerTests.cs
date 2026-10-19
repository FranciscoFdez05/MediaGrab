using MediaGrab.Models;
using MediaGrab.Services;
using Xunit;

namespace MediaGrab.Tests;

public class DownloadProgressTrackerTests
{
    private static DownloadProgressTracker Mp4() => new(new DownloadRequest("https://example.com", "C:\\Descargas", DownloadFormat.Mp4, 0));

    private static DownloadProgressTracker Mp3() => new(new DownloadRequest("https://example.com", "C:\\Descargas", DownloadFormat.Mp3, 0));

    [Fact]
    public void VideoYAudioSeparados_CadaArchivoEsLaMitadDelProgreso()
    {
        DownloadProgressTracker tracker = Mp4();
        tracker.Process("[info] abc: Downloading 1 format(s): 401+258");
        tracker.Process("[download] Destination: C:\\Descargas\\Video.f401.mp4");

        Assert.Equal(25, tracker.Process("[download]  50.0% of 10MiB at 1MiB/s ETA 00:05")?.Percent);

        tracker.Process("[download] Destination: C:\\Descargas\\Video.f258.m4a");
        Assert.Equal(75, tracker.Process("[download]  50.0% of 1MiB at 1MiB/s ETA 00:01")?.Percent);
    }

    [Fact]
    public void UnSoloArchivo_NoSeQuedaEnElCincuentaPorCiento()
    {
        DownloadProgressTracker tracker = Mp4();
        tracker.Process("[info] abc: Downloading 1 format(s): 18");
        tracker.Process("[download] Destination: C:\\Descargas\\Video.mp4");

        Assert.Equal(50, tracker.Process("[download]  50.0% of 10MiB at 1MiB/s ETA 00:05")?.Percent);
    }

    [Fact]
    public void ElProgresoNuncaRetrocede()
    {
        DownloadProgressTracker tracker = Mp3();
        tracker.Process("[download] Destination: a.webm");
        tracker.Process("[download]  60.0% of 1MiB at 1MiB/s ETA 00:01");

        DownloadProgress? update = tracker.Process("[download]  10.0% of 1MiB at 1MiB/s ETA 00:01");

        Assert.Equal(60, update?.Percent);
    }

    [Fact]
    public void AdmiteDecimalesConComa()
    {
        DownloadProgressTracker tracker = Mp3();
        tracker.Process("[download] Destination: a.webm");

        Assert.Equal(43, tracker.Process("[download]  42,6% of 1MiB at 1MiB/s ETA 00:01")?.Percent);
    }

    [Fact]
    public void MuestraVelocidadYTiempoRestante()
    {
        DownloadProgressTracker tracker = Mp3();
        tracker.Process("[download] Destination: a.webm");

        DownloadProgress? update = tracker.Process("[download]  42.0% of ~ 12.34MiB at  1.23MiB/s ETA 00:10 (frag 3/20)");

        Assert.Equal("42% · 1.23MiB/s · quedan 00:10", update?.Status);
    }

    [Fact]
    public void OcultaVelocidadDesconocida()
    {
        DownloadProgressTracker tracker = Mp3();
        tracker.Process("[download] Destination: a.webm");

        Assert.Equal("0%", tracker.Process("[download]   0.0% of 1MiB at Unknown B/s ETA Unknown")?.Status);
    }

    [Fact]
    public void ExtraeElTituloSinElSufijoDeFormato()
    {
        DownloadProgressTracker tracker = Mp4();

        DownloadProgress? update = tracker.Process("[download] Destination: C:\\Descargas\\Mi vídeo.f401.mp4");

        Assert.Equal("Mi vídeo", update?.Title);
    }

    [Fact]
    public void NoRepiteEstadosIguales()
    {
        DownloadProgressTracker tracker = Mp3();
        tracker.Process("[download] Destination: a.webm");
        tracker.Process("[download]  50.0% of 1MiB at 1MiB/s ETA 00:01");

        Assert.Null(tracker.Process("[download]  50.0% of 1MiB at 1MiB/s ETA 00:01"));
    }

    [Theory]
    [InlineData("[Merger] Merging formats into \"a.mp4\"", "Uniendo vídeo y audio...")]
    [InlineData("[ExtractAudio] Destination: a.mp3", "Convirtiendo a MP3...")]
    [InlineData("[FixupM3u8] Fixing MPEG-TS in MP4 container", "Procesando archivo...")]
    public void TraduceLasFasesDePostprocesado(string line, string expected)
    {
        DownloadProgressTracker tracker = Mp3();
        tracker.Process("[download] Destination: a.webm");

        Assert.Equal(expected, tracker.Process(line)?.Status);
    }
}
