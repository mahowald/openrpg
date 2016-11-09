using UnityEngine;
using System.Collections;

/// <summary>
/// A thin relay component to connect 
/// button presses to event triggers.
/// </summary>
public class ButtonRelay : MonoBehaviour {

	public void TriggerEvent(string s)
    {
        EventManager.TriggerEvent(s);
    }

    public void TriggerIntEvent(string s, int i)
    {
        EventManager.TriggerEvent<int>(s, i);
    }
}
