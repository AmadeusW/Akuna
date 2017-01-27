using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Akuna.Sandbox.Dispatchers
{
    interface IControlDispatcher
    {
        void Dispatch(object message);
        void Start();
        void Stop();
    }
}
