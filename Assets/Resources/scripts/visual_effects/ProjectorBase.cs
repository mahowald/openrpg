using UnityEngine;
using System.Collections;

public class ProjectorBase : MonoBehaviour {

    public GameObject projectorTop;
    public Transform follow;

    // Use this for initialization
    void Start () {
	    
	}
    
    // Update is called once per frame
    void Update ()
    {
        this.transform.position = follow.position;
        LookAtMouse();
    }

    bool on = false;
    void SetFollowActor(ActorSystem.Actor a)
    {
        follow = a.transform;
    }
    public void SetProjector()
    {
        on = !on;
        projectorTop.SetActive(on);
    }

    public void LookAtMouse()
    {
        GetMousePosition();
        transform.rotation = Quaternion.LookRotation(mousePosition - transform.position, Vector3.up);
    }

    Vector3 mousePosition = Vector3.zero;

    void GetMousePosition()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            mousePosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
        }
    }

    // Subscribe/Unsubscribe to our events
    public void OnEnable()
    {
        EventManager.StartListening("ButtonPressed", SetProjector);
        EventManager.StartListening<ActorSystem.Actor>("ActorClicked", SetFollowActor);
    }
    public void OnDisable()
    {
        EventManager.StopListening("ButtonPressed", SetProjector);
        EventManager.StopListening<ActorSystem.Actor>("ActorClicked", SetFollowActor);
    }


}
