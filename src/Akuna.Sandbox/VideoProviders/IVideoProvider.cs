using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Akuna.Sandbox.VideoProviders
{
    interface IVideoProvider
    {
        bool Attached { get; }
        string WindowTitle { get; }

        void Attach(Process process);
        ImageSource GetVideo();
        void Detach();
    }
}
