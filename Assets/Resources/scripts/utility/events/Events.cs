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