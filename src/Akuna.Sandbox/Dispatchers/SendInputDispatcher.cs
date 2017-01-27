using System;
using System.Collections.Generic;
using System.Timers;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Akuna.Sandbox.Dispatchers
{
    /// <summary>
    /// A test dispatcher that repeats messages every 5 seconds.
    /// </summary>
    class DelayDispatcher : IControlDispatcher
    {
        Queue<PInvoke.User32.WindowMessage> _messages = new Queue<PInvoke.User32.WindowMessage>();
        Timer _timer = new Timer(5000);

        internal DelayDispatcher()
        {
            _timer.Elapsed += DelayedDispatch;
        }

        private void DelayedDispatch(object sender, ElapsedEventArgs e)
        {
            while (_messages.Any())
            {
                var message = _messages.Dequeue();
                Debug.WriteLine(message);
                //PInvoke.User32.SendInput(1, new PInvoke.User32.MOUSEINPUT() {dwFlags = message }, );
            };
        }

        public void Dispatch(object message)
        {
            _messages.Enqueue((PInvoke.User32.WindowMessage)message);
        }

        public void Start()
        {
            _timer.Start();
            
        }

        public void Stop()
        {
            _timer.Stop();
        }
    }
}
