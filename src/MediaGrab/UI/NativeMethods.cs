using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MediaGrab.UI
{
    /// <summary>
    /// Funciones de Windows que WinForms (.NET Framework) no expone directamente.
    /// </summary>
    internal static class NativeMethods
    {
        private const int EM_SETCUEBANNER = 0x1501;
        private const uint FLASHW_ALL = 0x3;
        private const uint FLASHW_TIMERNOFG = 0xC;

        /// <summary>
        /// Muestra un texto de ejemplo en gris dentro de un TextBox vacío (también con el foco).
        /// </summary>
        public static void SetPlaceholder(TextBox textBox, string text)
        {
            SendMessage(textBox.Handle, EM_SETCUEBANNER, (IntPtr)1, text);
        }

        /// <summary>
        /// Hace parpadear el botón de la barra de tareas hasta que el usuario vuelva a la ventana.
        /// </summary>
        public static void FlashUntilActive(Form form)
        {
            FLASHWINFO info = new FLASHWINFO
            {
                cbSize = (uint)Marshal.SizeOf(typeof(FLASHWINFO)),
                hwnd = form.Handle,
                dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG,
                uCount = uint.MaxValue,
                dwTimeout = 0
            };

            FlashWindowEx(ref info);
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }
    }
}
