using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Game controller class
public class GameController : MonoBehaviour {

    public enum ViewMode { Standard, FreeLook }; 
    private static ViewMode viewMode = ViewMode.Standard;

    private static GameController gameController;

    private static Transform gameCamera;
    
    public static GameController instance
    {
        get
        {
            return gameController;
        }
    }

	// Use this for initialization
	void Start () {
        gameController = this;
        gameCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Transform>();
	}
	
	// Update is called once per frame
	void Update () {
        Do3DMouseClick();
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

    void Do3DMouseClick()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                EventManager.TriggerEvent<Vector3>("MouseClickLocation3D", hit.point);
            }
        }
    }
    
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
