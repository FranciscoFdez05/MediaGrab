using System;
using System.Runtime.InteropServices;

namespace MediaGrab.UI
{
    internal enum TaskbarProgressState
    {
        None = 0x0,
        Indeterminate = 0x1,
        Normal = 0x2,
        Error = 0x4,
        Paused = 0x8
    }

    /// <summary>
    /// Muestra el progreso en el icono de la barra de tareas (ITaskbarList3), como hacen
    /// los navegadores o el Explorador de Windows al copiar archivos.
    /// </summary>
    internal static class TaskbarProgress
    {
        private static readonly ITaskbarList3 Taskbar = Create();

        public static void SetState(IntPtr windowHandle, TaskbarProgressState state)
        {
            Invoke(() => Taskbar.SetProgressState(windowHandle, state));
        }

        public static void SetValue(IntPtr windowHandle, int value, int maximum)
        {
            Invoke(() => Taskbar.SetProgressValue(windowHandle, (ulong)Math.Max(0, value), (ulong)maximum));
        }

        private static void Invoke(Action action)
        {
            if (Taskbar == null)
            {
                return;
            }

            try
            {
                action();
            }
            catch (COMException)
            {
                // El explorador puede no estar disponible (p.ej. al reiniciarse): es solo decorativo.
            }
        }

        private static ITaskbarList3 Create()
        {
            try
            {
                ITaskbarList3 taskbar = (ITaskbarList3)new TaskbarInstance();
                taskbar.HrInit();
                return taskbar;
            }
            catch (Exception ex) when (ex is COMException || ex is InvalidCastException)
            {
                return null;
            }
        }

        [ComImport]
        [Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface ITaskbarList3
        {
            // ITaskbarList
            [PreserveSig] void HrInit();
            [PreserveSig] void AddTab(IntPtr hwnd);
            [PreserveSig] void DeleteTab(IntPtr hwnd);
            [PreserveSig] void ActivateTab(IntPtr hwnd);
            [PreserveSig] void SetActiveAlt(IntPtr hwnd);

            // ITaskbarList2
            [PreserveSig] void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fullscreen);

            // ITaskbarList3
            [PreserveSig] void SetProgressValue(IntPtr hwnd, ulong completed, ulong total);
            [PreserveSig] void SetProgressState(IntPtr hwnd, TaskbarProgressState state);
        }

        [ComImport]
        [Guid("56fdf344-fd6d-11d0-958a-006097c9a090")]
        [ClassInterface(ClassInterfaceType.None)]
        private class TaskbarInstance
        {
        }
    }
}
