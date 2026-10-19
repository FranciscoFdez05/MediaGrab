using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using MediaGrab.UI;

namespace MediaGrab
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            EnsureDpiAwareness();

            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += OnThreadException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        /// <summary>
        /// Con MediaGrab.exe.config, WinForms activa el escalado por monitor (PerMonitorV2).
        /// Si alguien ejecuta el .exe suelto, sin el .config, se activa al menos el escalado
        /// por sistema para que la ventana no se vea borrosa en pantallas con escalado.
        /// </summary>
        private static void EnsureDpiAwareness()
        {
            if (File.Exists(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile))
            {
                return;
            }

            try
            {
                SetProcessDpiAwarenessContext(DpiAwarenessContextSystemAware);
            }
            catch (EntryPointNotFoundException)
            {
                // Windows anterior a 10 (1703): se queda con el comportamiento por defecto.
            }
        }

        private static readonly IntPtr DpiAwarenessContextSystemAware = new IntPtr(-2);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetProcessDpiAwarenessContext(IntPtr value);

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
}
