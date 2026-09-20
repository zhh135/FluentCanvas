using System;
using System.Diagnostics;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace FluentCanvas.Helpers
{
    internal class ForegroundWindowInfo
    {
        public static string WindowTitle()
        {
            HWND foregroundWindowHandle = PInvoke.GetForegroundWindow();

            Span<char> windowTitle = stackalloc char[256];
            int length = PInvoke.GetWindowText(foregroundWindowHandle, windowTitle);

            return windowTitle.Slice(0, length).ToString();
        }

        public static string WindowClassName()
        {
            HWND foregroundWindowHandle = PInvoke.GetForegroundWindow();

            Span<char> className = stackalloc char[256];
            int length = PInvoke.GetClassName(foregroundWindowHandle, className);

            return className.Slice(0, length).ToString();
        }

        public static RECT WindowRect()
        {
            HWND foregroundWindowHandle = PInvoke.GetForegroundWindow();

            PInvoke.GetWindowRect(foregroundWindowHandle, out RECT windowRect);

            return windowRect;
        }

        public static string ProcessName()
        {
            HWND foregroundWindowHandle = PInvoke.GetForegroundWindow();
            PInvoke.GetWindowThreadProcessId(foregroundWindowHandle, out uint processId);

            try
            {
                Process process = Process.GetProcessById((int)processId);
                return process.ProcessName;
            }
            catch (ArgumentException)
            {
                // Process with the given ID not found
                return "Unknown";
            }
        }
    }
}
