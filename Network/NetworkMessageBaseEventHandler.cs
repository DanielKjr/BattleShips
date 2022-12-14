using System;
using System.Collections.Generic;

namespace BattleShips
{
    public class NetworkMessageBaseEventHandler
    {

        public delegate void EventDelegate<T>(T e) where T : NetworkMessageBase;
        private delegate void EventDelegate(NetworkMessageBase e);

        private Dictionary<Type, EventDelegate> delegates = new Dictionary<Type, EventDelegate>();
        private Dictionary<Delegate, EventDelegate> delegateLookup = new Dictionary<Delegate, EventDelegate>();

        public void AddListener<T>(EventDelegate<T> del) where T : NetworkMessageBase
        {

            if (delegateLookup.ContainsKey(del))
                return;


            EventDelegate internalDelegate = (e) => del((T)e);
            delegateLookup[del] = internalDelegate;

            EventDelegate tempDel;
            if (delegates.TryGetValue(typeof(T), out tempDel))
            {
                delegates[typeof(T)] = tempDel += internalDelegate;
            }
            else
            {
                delegates[typeof(T)] = internalDelegate;
            }
        }

        public void RemoveListener<T>(EventDelegate<T> del) where T : NetworkMessageBase
        {
            EventDelegate internalDelegate;
            if (delegateLookup.TryGetValue(del, out internalDelegate))
            {
                EventDelegate tempDel;
                if (delegates.TryGetValue(typeof(T), out tempDel))
                {
                    tempDel -= internalDelegate;
                    if (tempDel == null)
                    {
                        delegates.Remove(typeof(T));
                    }
                    else
                    {
                        delegates[typeof(T)] = tempDel;
                    }

                }

                delegateLookup.Remove(del);
            }
        }

        public void Raise(NetworkMessageBase e)
        {
            EventDelegate del;
            if (delegates.TryGetValue(e.GetType(), out del))
            {
                del.Invoke(e);
            }
        }

    }
}
