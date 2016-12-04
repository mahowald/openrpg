using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Valve.VR;

// Game controller class
public class GameController : MonoBehaviour {

    private static bool vrEnabled = false;
    private static Transform standardCameraRig;
    private static Transform vrCameraRig;
    public static bool VREnabled
    {
        set {
            vrEnabled = value;
            vrCameraRig.gameObject.SetActive(vrEnabled);
            standardCameraRig.gameObject.SetActive(!vrEnabled);
        }
        get { return vrEnabled; }
    }

    public enum ViewMode { Standard, FreeLook }; 
    private static ViewMode viewMode = ViewMode.Standard;
    private static List<ActorSystem.Actor> actors;

    public static void AddActor(ActorSystem.Actor a)
    {
        if (actors.Contains(a))
            return;

        actors.Add(a);
    }

    public static List<ActorSystem.Actor> Actors
    {
        get { return actors; }
    }

    public static ViewMode VMode
    {
        get { return viewMode; }
    }

    private static GameController gameController;

    private static Transform gameCamera;
    
    public static GameController instance
    {
        get
        {
            return gameController;
        }
    }

    void Awake()
    {
        if(VREnabled)
        {
            StartCoroutine(LoadDevice("OpenVR"));
        }
        else
        {
            StartCoroutine(LoadDevice(""));
        }
    }

    IEnumerator LoadDevice(string newDevice)
    {
        UnityEngine.VR.VRSettings.LoadDeviceByName(newDevice);
        yield return null;
        UnityEngine.VR.VRSettings.enabled = VREnabled;
    }

    // Use this for initialization
    void Start() {
        gameController = this;
        vrCameraRig = GameObject.FindGameObjectWithTag("VRCamera").transform;
        standardCameraRig = GameObject.FindGameObjectWithTag("StandardCamera").transform;
        if (VREnabled)
        {
            standardCameraRig.gameObject.SetActive(false);
            gameCamera = vrCameraRig.GetComponentInChildren<Camera>().transform;
        }
        else
        {
            vrCameraRig.gameObject.SetActive(false);
            gameCamera = standardCameraRig.GetComponentInChildren<Camera>().transform;
        }
        actors = new List<ActorSystem.Actor>(GameObject.FindObjectsOfType<ActorSystem.Actor>());


	}
	
	// Update is called once per frame
	void Update () {
        Do3DMouseSelect();
        TogglePause();
        ToggleViewMode();
	}

    // Handle requests to pause/unpause the game
    bool paused = false;
    void TogglePause()
    {
        if (Input.GetButtonUp("Pause"))
        {
            if (!paused)
            {
                paused = true;
                EventManager.TriggerEvent("Pause");
            }
            else if (paused)
            {
                paused = false;
                EventManager.TriggerEvent("Unpause");
            }
        }
    }

    // Determine where the mouse clicked
    // Also, highlight current mouse position
    ActorSystem.Actor lastActor = null;
    void Do3DMouseSelect()
    {
        if (viewMode == ViewMode.Standard && !EventSystem.current.IsPointerOverGameObject() && !SelectorBase.CurrentlySelecting)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            bool raycast = Physics.Raycast(ray, out hit);

            ActorSystem.Actor actor = null;
            if (raycast)
            {
                actor = hit.transform.GetComponent<ActorSystem.Actor>();
                if (actor != null)
                {
                    actor.Highlighted = true;
                    if (actor != lastActor && lastActor != null)
                        lastActor.Highlighted = false;
                    lastActor = actor;
                }
                else
                {
                    if (lastActor != null)
                        lastActor.Highlighted = false;
                    lastActor = null;
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (actor == null) // check that we didn't click on an actor
                        EventManager.TriggerEvent<Vector3>("MouseClickLocation3D", hit.point);
                }
            }
            else
            {
                if (lastActor != null)
                    lastActor.Highlighted = false;
                lastActor = null;
            }

            
        }
    }
    
    // Convert the movement input into a 3-vector direction
    public static Vector3 Get3DMovementDir()
    {
        float moveInX = Input.GetAxis("Horizontal");
        float moveInY = Input.GetAxis("Vertical");

        Vector3 cameraForward = new Vector3(gameCamera.forward.x, 0, gameCamera.forward.z).normalized;
        Vector3 cameraRight = new Vector3(gameCamera.right.x, 0, gameCamera.right.z).normalized;

        return moveInX*cameraRight + moveInY*cameraForward;
    }
    
    void ToggleViewMode()
    {
        if(Input.GetButtonUp("ViewMode") && !orthoOn)
        {
            if ( viewMode == ViewMode.Standard)
            {
                viewMode = ViewMode.FreeLook;
                EventManager.TriggerEvent("ViewMode: FreeLook");
                Cursor.visible = false;
            } else
            {
                viewMode = ViewMode.Standard;
                EventManager.TriggerEvent("ViewMode: Standard");
                Cursor.visible = true;
            }
        }
    }

    private bool orthoOn = false;
    void SetOrthographicMode(bool orthographicMode)
    {
        orthoOn = orthographicMode;
        if(orthoOn && viewMode == ViewMode.FreeLook)
        {
            viewMode = ViewMode.Standard;
            EventManager.TriggerEvent("ViewMode: Standard");
            Cursor.visible = true;
        }
    }

    // Subscribe/Unsubscribe to our events
    public void OnEnable()
    {
        EventManager.StartListening<bool>("Orthographic Mode", SetOrthographicMode);
    }
    public void OnDisable()
    {
        EventManager.StopListening<bool>("Orthographic Mode", SetOrthographicMode);
    }

}
