using System.Collections.Generic;
using System.Linq;

namespace MediaGrab.Models;

/// <summary>
/// Forma de invocar yt-dlp en este equipo: un ejecutable directo (yt-dlp.exe)
/// o un intérprete de Python con el módulo (python -m yt_dlp).
/// </summary>
internal sealed class YtDlpCommand(string executable, params string[] prefixArguments)
{
    public string Executable { get; } = executable;

    public IReadOnlyList<string> PrefixArguments { get; } = prefixArguments ?? [];

    /// <summary>True cuando yt-dlp se ejecuta como módulo de Python (solo se actualiza con pip).</summary>
    public bool IsPythonModule => PrefixArguments.Count > 0;

    public IEnumerable<string> WithArguments(params string[] arguments)
    {
        return PrefixArguments.Concat(arguments);
    }

    /// <summary>Línea de comandos legible, solo para mostrarla al usuario.</summary>
    public override string ToString()
    {
        return string.Join(" ", new[] { Executable }.Concat(PrefixArguments)
            .Select(part => part.Contains(' ') ? $"\"{part}\"" : part));
    }
}
