using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SelectorBase : MonoBehaviour {

    public GameObject projectorTop;
    public Transform follow;

    public static SelectorBase instance;

    HashSet<ActorSystem.Actor> selectedActors;

    private float radius = 5f;
    private float arcAngle = 30f;

    public float Radius
    {
        get { return radius; }
        set {
            radius = value;
            projectorTop.transform.localPosition = new Vector3(0, radius, 0);
        }
    }

    public float ArcAngle
    {
        get { return arcAngle; }
        set { arcAngle = value; }
    }

    // Use this for initialization
    void Start () {
        if (instance == null)
            instance = this;
        else
            Debug.LogError("There can only be one Selector in the scene.");
        selectedActors = new HashSet<ActorSystem.Actor>();
	}
    
    // Update is called once per frame
    void Update ()
    {
        this.transform.position = follow.position;
        LookAtMouse();

        if (on)
        {
            UpdateSelection();
            if(Input.GetMouseButtonUp(0) ) // left click
            {
                StopSelection();
                EventManager.TriggerEvent<Geometry.Arc>("Arc Selected", arc);
            }
            if(Input.GetMouseButtonUp(1)) // right click
            {
                StopSelection();
            }
        }
    }

    bool on = false;
    void SetFollowActor(ActorSystem.Actor a)
    {
        follow = a.transform;
        currentActor = a;
    }

    ActorSystem.Actor currentActor;
    ActorSystem.Actor sourceActor;
    public static ActorSystem.Actor SourceActor
    {
        get { return instance.sourceActor; }
    }

    public void SetProjector()
    {
        on = !on;
        projectorTop.SetActive(on);
        if (!on)
            ClearSelection();
    }

    private Geometry.Arc arc;

    public static Geometry.Arc ArcSelection
    {
        get { return instance.arc; }
    }

    void UpdateSelection()
    {
        arc = new Geometry.Arc(radius, arcAngle, this.transform.position, this.transform.forward);
        List<ActorSystem.Actor> arcActors = Geometry.GetActorsInArc(arc, ActorFilter);
        HashSet<ActorSystem.Actor> previousActors = new HashSet<ActorSystem.Actor>(selectedActors);
        foreach(ActorSystem.Actor a in previousActors)
        {
            if(!arcActors.Contains(a))
            {
                a.Highlighted = false;
                selectedActors.Remove(a);
            }
        }
        foreach(ActorSystem.Actor a in arcActors)
        {
            if(!selectedActors.Contains(a))
            {
                a.Highlighted = true;
                selectedActors.Add(a);
            }
        }
    }

    void ClearSelection()
    {
        foreach(ActorSystem.Actor a in this.selectedActors)
        {
            a.Highlighted = false;
        }
        selectedActors.Clear();
    }

    private bool ActorFilter(ActorSystem.Actor a)
    {
        if (a.transform == follow)
            return false;
        return true;
    }

    public void LookAtMouse()
    {
        GetMousePosition();
        transform.rotation = Quaternion.LookRotation(mousePosition - transform.position, Vector3.up);
    }

    Vector3 mousePosition = Vector3.zero;

    void GetMousePosition()
    {
        mousePosition = GameController.MousePosition;
        return;
        /**
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            mousePosition = new Vector3(hit.point.x, transform.position.y, hit.point.z);
        }
    **/
    }

    void StartSelection()
    {
        on = true;
        projectorTop.SetActive(true);
    }

    void StopSelection()
    {
        on = false;
        projectorTop.SetActive(false);
        ClearSelection();
        sourceActor = currentActor;
    }

    public static bool CurrentlySelecting
    {
        get { return instance.on;  }
    }

    // Subscribe/Unsubscribe to our events
    public void OnEnable()
    {
        EventManager.StartListening("AbilityButtonPressed", StartSelection);
        EventManager.StartListening<ActorSystem.Actor>("ActorClicked", SetFollowActor);
    }
    public void OnDisable()
    {
        EventManager.StopListening("AbilityButtonPressed", SetProjector);
        EventManager.StopListening<ActorSystem.Actor>("ActorClicked", SetFollowActor);
    }
    


}
