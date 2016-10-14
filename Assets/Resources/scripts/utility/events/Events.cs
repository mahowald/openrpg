using UnityEngine;
using UnityEngine.Events;
using System.Collections;

namespace Events
{
    // Here we will define all of our event types...
    public class Vector3Event : UnityEvent<Vector3>
    {
        public Vector3Event() : base()
        {
        }
    }

    public class FloatEvent : UnityEvent<float>
    {
        public FloatEvent() : base() { }
    }


}