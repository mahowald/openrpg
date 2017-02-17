using UnityEngine;
using System.Collections;

public class UIBase : MonoBehaviour {

    private Vector3 pointContext;
    private ActorSystem.Actor actorContext;

    private Canvas canvas;

	// Use this for initialization
	void Start () {
        canvas = this.gameObject.GetComponentInChildren<Canvas>();
        canvas.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void Relocate(Vector3 dest)
    {
        this.transform.position = dest;
    }

    void ActorSelected(ActorSystem.Actor actor)
    {
        canvas.gameObject.SetActive(true);
        actorContext = actor;
        Relocate(actor.Position);
    }

    void PointSelected(Vector3 point)
    {
        canvas.gameObject.SetActive(true);
        pointContext = point;
        actorContext = null;
        Relocate(point);
    }

    // Subscribe/Unsubscribe to our events
    public void OnEnable()
    {
        EventManager.StartListening<ActorSystem.Actor>("ContextClick", ActorSelected);
        EventManager.StartListening<Vector3>("ContextClick", PointSelected);
    }
    public void OnDisable()
    {
        EventManager.StopListening<ActorSystem.Actor>("ContextClick", ActorSelected);
        EventManager.StopListening<Vector3>("ContextClick", PointSelected);
    }

    // buttons
    public void Swap()
    {
        if(actorContext != null)
        {
            EventManager.TriggerEvent("ActorClicked", actorContext);
            actorContext = null;
            canvas.gameObject.SetActive(false);
        }
    }
}
