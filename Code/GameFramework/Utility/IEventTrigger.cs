using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Utility
{
    interface IEventTrigger<T>
    {
        void AddEventListener(Action<T> listener);
        void RemoveEventListener();
    }
}
