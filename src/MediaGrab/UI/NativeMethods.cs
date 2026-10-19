using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MediaGrab.UI;

/// <summary>
/// Funciones de Windows que WinForms no expone directamente.
/// </summary>
internal static partial class NativeMethods
{
    private const uint FLASHW_ALL = 0x3;
    private const uint FLASHW_TIMERNOFG = 0xC;

    /// <summary>
    /// Hace parpadear el botón de la barra de tareas hasta que el usuario vuelva a la ventana.
    /// </summary>
    public static void FlashUntilActive(Form form)
    {
        FLASHWINFO info = new()
        {
            cbSize = (uint)Marshal.SizeOf<FLASHWINFO>(),
            hwnd = form.Handle,
            dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG,
            uCount = uint.MaxValue,
            dwTimeout = 0
        };

        FlashWindowEx(ref info);
    }

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool FlashWindowEx(ref FLASHWINFO pwfi);

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
