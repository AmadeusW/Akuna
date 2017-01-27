using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PInvoke;

namespace Akuna.Sandbox.VideoProviders
{
    class Win32VideoProvider : IVideoProvider
    {
        private bool _attached;
        private IntPtr _hwnd;
        private string _windowTitle;

        bool IVideoProvider.Attached => _attached;
        string IVideoProvider.WindowTitle => _windowTitle ?? String.Empty;

        // From http://stackoverflow.com/questions/1953582/how-to-i-get-the-window-handle-by-giving-the-process-name-that-is-running
        void IVideoProvider.Attach(Process process)
        {
            _hwnd = process.MainWindowHandle;
            if (_hwnd.Equals(IntPtr.Zero))
            {
                _hwnd = GetWindowHandleLong(process);
                if (_hwnd.Equals(IntPtr.Zero))
                {
                    throw new ArgumentException($"Process {process.ProcessName} doesn't have main window handle.");
                }
            }
            _windowTitle = process.MainWindowTitle;
            _attached = true;
        }

        // From https://social.msdn.microsoft.com/Forums/windowsdesktop/en-US/7e25e104-36cb-41ac-8f36-0e4c6b6146a3/finding-hwnd-of-metro-app-using-win32-api?forum=windowsgeneraldevelopmentissues
        // and https://social.msdn.microsoft.com/Forums/windowsdesktop/en-US/fdc14663-4ab8-484b-aae5-4639e0699624/how-to-get-window-handle-for-windows-store-apps-using-win32-api?forum=windowsgeneraldevelopmentissues
        // and doesn't work
        private IntPtr GetWindowHandleLong(Process process)
        {
            // Now need to run a loop to get the correct window for process.
            bool bFound = false;
            IntPtr prevWindow = IntPtr.Zero;

            while (!bFound)
            {
                IntPtr desktopWindow = User32.GetDesktopWindow();
                if (desktopWindow == IntPtr.Zero)
                    break;

                IntPtr nextWindow = User32.FindWindowEx(desktopWindow, prevWindow, null, null);
                if (nextWindow == IntPtr.Zero)
                    break;

                // Check whether window belongs to the correct process.
                int procId;
                User32.GetWindowThreadProcessId(nextWindow, out procId);

                if (procId == process.Id)
                {
                    return nextWindow;
                }

                prevWindow = nextWindow;
            }
            return IntPtr.Zero;
        }

        void IVideoProvider.Detach()
        {
            _attached = false;
        }

        // From http://stackoverflow.com/questions/891345/get-a-screenshot-of-a-specific-application
        ImageSource IVideoProvider.GetVideo()
        {
            RECT rc;
            User32.GetWindowRect(_hwnd, out rc);

            using (Bitmap bmp = new Bitmap(rc.right-rc.left, rc.bottom-rc.top, System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            using (Graphics gfxBmp = Graphics.FromImage(bmp))
            {
                IntPtr hdcBitmap = gfxBmp.GetHdc();
                try
                {
                    var status = User32.PrintWindow(_hwnd, hdcBitmap, User32.PrintWindowFlags.PW_CLIENTONLY);
                }
                finally
                {
                    gfxBmp.ReleaseHdc(hdcBitmap);
                }
                var source = loadBitmap(bmp);
                return source;
            }
        }

        // From http://stackoverflow.com/questions/1118496/using-image-control-in-wpf-to-display-system-drawing-bitmap
        private static BitmapSource loadBitmap(Bitmap source)
        {
            IntPtr ip = source.GetHbitmap();
            BitmapSource bs = null;
            try
            {
                bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(ip,
                   IntPtr.Zero, Int32Rect.Empty,
                   System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
            }
            finally
            {
                Gdi32.DeleteObject(ip);
            }

            return bs;
        }
    }
}
