using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MediaGrab.Infrastructure
{
    /// <summary>
    /// Construye líneas de comandos de Windows a partir de una lista de argumentos,
    /// escapándolos según las reglas de CommandLineToArgvW.
    /// </summary>
    internal static class CommandLine
    {
        private static readonly char[] CharsRequiringQuotes = { ' ', '\t', '\n', '\v', '"' };

        public static string Join(IEnumerable<string> arguments)
        {
            return string.Join(" ", arguments.Select(Quote));
        }

        public static string Quote(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                return "\"\"";
            }

            if (argument.IndexOfAny(CharsRequiringQuotes) < 0)
            {
                return argument;
            }

            StringBuilder builder = new StringBuilder(argument.Length + 2);
            builder.Append('"');

            int backslashes = 0;

            foreach (char c in argument)
            {
                if (c == '\\')
                {
                    backslashes++;
                    continue;
                }

                if (c == '"')
                {
                    // Las barras que preceden a una comilla se duplican y la comilla se escapa.
                    builder.Append('\\', backslashes * 2 + 1);
                }
                else
                {
                    builder.Append('\\', backslashes);
                }

                builder.Append(c);
                backslashes = 0;
            }

            // Las barras finales se duplican para no escapar la comilla de cierre.
            builder.Append('\\', backslashes * 2);
            builder.Append('"');

            return builder.ToString();
        }
    }
}
