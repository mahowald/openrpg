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
        EventManager.StartListening<Vector3>("MouseClickLocation3D", SetDestination);
    }
    public void OnDisable()
    {
        EventManager.StopListening<Vector3>("MouseClickLocation3D", SetDestination);
    }

    public void SetDestination(Vector3 destination)
    {
        actor.Destination = destination;
    }
}
