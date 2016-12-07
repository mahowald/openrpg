using UnityEngine;
using System.Collections;

public interface ILocatable
{
    Vector3 Position { get; } // the current position
    Vector3 Direction { get; } // the direction to be facing
}
