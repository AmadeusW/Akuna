using Akuna.Sandbox.Dispatchers;
using Akuna.Sandbox.VideoProviders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WindowsInput;

namespace Akuna.Sandbox
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IVideoProvider _video;
        IControlDispatcher _dispatcher;
        bool _working;

        public MainWindow()
        {
            InitializeComponent();
            _video = new Win32VideoProvider();
            _dispatcher = new DelayDispatcher();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                if (!_video.Attached)
                {
                    Process process = Process.GetProcessesByName("devenv").FirstOrDefault();
                    if (process == null)
                    {
                        return;
                    }
                    this.Title = _video.WindowTitle;
                    _video.Attach(process);
                    _dispatcher.Start();
                    _working = true;
                }
                VideoCanvas.Source = _video.GetVideo();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _video?.Detach();
            _dispatcher.Stop();
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.RemoveHook(WndProc);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
            source.AddHook(WndProc);
        }

        // Handle messages. 
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (!_working)
                return IntPtr.Zero;

            PInvoke.User32.WindowMessage wMsg = (PInvoke.User32.WindowMessage)msg;
            //System.Diagnostics.Debug.WriteLine($"{hwnd} \t{wMsg} \t{wParam} \t{lParam}");
            /*if (wMsg == PInvoke.User32.WindowMessage.WM_MOUSEMOVE
                || wMsg == PInvoke.User32.WindowMessage.WM_NCMOUSEMOVE)
            {*/
            _dispatcher?.Dispatch(msg);
            //}
            //var x = new InputSimulator();
            //x.Keyboard.KeyPress(WindowsInput.Native.VirtualKeyCode.SPACE);


            /*if (_handle != IntPtr.Zero)
            {
                //PInvoke.User32.ShowWindow(_handle, PInvoke.User32.WindowShowStyle.SW_SHOWNORMAL);
                //PInvoke.User32.PostMessage(_handle, wMsg, wParam, lParam);
                PInvoke.User32.PostMessage(_handle, PInvoke.User32.WindowMessage.WM_KEYDOWN, new IntPtr(5), IntPtr.Zero);
                PInvoke.User32.PostMessage(_handle, PInvoke.User32.WindowMessage.WM_KEYUP, new IntPtr(5), IntPtr.Zero);
            }*/
            return IntPtr.Zero;
        }
    }
}
