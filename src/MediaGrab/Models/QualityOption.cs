using System.Collections.Generic;

namespace MediaGrab.Models
{
    /// <summary>
    /// Opción del selector de calidad. <see cref="Value"/> se interpreta según el formato
    /// (ver <see cref="DownloadRequest.Quality"/>).
    /// </summary>
    internal sealed class QualityOption
    {
        private QualityOption(string text, int value)
        {
            Text = text;
            Value = value;
        }

        public string Text { get; }

        public int Value { get; }

        public static IReadOnlyList<QualityOption> For(DownloadFormat format)
        {
            return format == DownloadFormat.Mp3 ? Audio : Video;
        }

        public override string ToString() => Text;

        private static readonly QualityOption[] Video =
        {
            new QualityOption("La mejor", 0),
            new QualityOption("1080p", 1080),
            new QualityOption("720p", 720),
            new QualityOption("480p", 480),
            new QualityOption("360p", 360)
        };

        private static readonly QualityOption[] Audio =
        {
            new QualityOption("La mejor", 0),
            new QualityOption("320 kbps", 320),
            new QualityOption("192 kbps", 192),
            new QualityOption("128 kbps", 128)
        };
    }
}
