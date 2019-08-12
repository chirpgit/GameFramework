using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Game.Utility
{
    interface IComponentable : IDisposable
    {
        void AddComponet(Component com);
    }

    abstract class Component
    {

    }

    public class EventProxy<T> where T:IDisposable
    {
        public EventProxy(T obj, EventHandler eventDelegate) 
        {

        }
    }
}
