using System.Collections.Generic;
using System.Linq;
using MediaGrab.Infrastructure;

namespace MediaGrab.Models
{
    /// <summary>
    /// Forma de invocar yt-dlp en este equipo: un ejecutable directo (yt-dlp.exe)
    /// o un intérprete de Python con el módulo (python -m yt_dlp).
    /// </summary>
    internal sealed class YtDlpCommand
    {
        public YtDlpCommand(string executable, params string[] prefixArguments)
        {
            Executable = executable;
            PrefixArguments = prefixArguments ?? new string[0];
        }

        public string Executable { get; }

        public IReadOnlyList<string> PrefixArguments { get; }

        /// <summary>True cuando yt-dlp se ejecuta como módulo de Python (solo se actualiza con pip).</summary>
        public bool IsPythonModule => PrefixArguments.Count > 0;

        public IEnumerable<string> WithArguments(params string[] arguments)
        {
            return PrefixArguments.Concat(arguments);
        }

        public override string ToString()
        {
            return CommandLine.Join(new[] { Executable }.Concat(PrefixArguments));
        }
    }
}
