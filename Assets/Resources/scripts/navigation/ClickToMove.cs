using UnityEngine;
using System.Collections;

// Deprecated -- handled in the Actor now.
public class ClickToMove : MonoBehaviour
{

    public Actor.Actor actor;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnEnable()
    {
        EventManager.StartListening("MouseClickLocation3D", SetDestination);
    }
    public void OnDisable()
    {
        EventManager.StopListening("MouseClickLocation3D", SetDestination);
    }

    public void SetDestination(Vector3 destination)
    {
        actor.Destination = destination;
    }
}
