using System;
using System.Threading;
using System.Windows.Forms;
using MediaGrab.UI;

namespace MediaGrab;

internal static class Program
{
    /// <summary>
    /// Punto de entrada principal para la aplicación.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
        Application.ThreadException += OnThreadException;

        // Escalado por monitor, estilos visuales y renderizado de texto (ver MediaGrab.csproj).
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }

    private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
    {
        // Último recurso: informar del error sin cerrar la aplicación de golpe.
        MessageBox.Show(
            $"Se ha producido un error inesperado:\n\n{e.Exception.Message}",
            "MediaGrab",
            MessageBoxButtons.OK,
            MessageBoxIcon.Error);
    }
}
