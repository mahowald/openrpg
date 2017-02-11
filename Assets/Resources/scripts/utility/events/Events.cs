using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Events
{
    public interface IOneArgEvent
    {
        void Invoke(object argument);
        void AddListener(object argument);
        void RemoveListener(object argument);
    }

    public interface ITwoArgEvent
    {
        void Invoke(object argument1, object argument2);
        void AddListener(object argument);
        void RemoveListener(object argument);
    }

    public class GenericEvent<T> : UnityEvent<T>, IOneArgEvent
    {
        public void Invoke(object argument)
        {
            base.Invoke((T)argument);
        }

        public void AddListener(object listener)
        {
            base.AddListener((UnityAction<T>)listener);
        }

        public void RemoveListener(object listener)
        {
            base.RemoveListener((UnityAction<T>)listener);
        }
        
    }

    public class GenericEvent<T0, T1> : UnityEvent<T0, T1>, ITwoArgEvent
    {
        public void Invoke(object argument1, object argument2)
        {
            base.Invoke((T0)argument1, (T1)argument2);
        }

        public void AddListener(object listener)
        {
            base.AddListener((UnityAction<T0, T1>)listener);
        }

        public void RemoveListener(object listener)
        {
            base.RemoveListener((UnityAction<T0, T1>)listener);
        }
    }

    // Here we will define all of our event types...
    public class Vector3Event : GenericEvent<Vector3>
    {
        public Vector3Event() : base()
        {
        }
    }

    public class FloatEvent : GenericEvent<float>
    {
        public FloatEvent() : base() { }
    }

    public class BoolEvent : GenericEvent<bool>
    {
        public BoolEvent() : base() { }
    }


}