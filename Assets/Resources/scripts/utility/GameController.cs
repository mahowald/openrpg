using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Game controller class
public class GameController : MonoBehaviour {

    private static GameController gameController;

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
	}
	
	// Update is called once per frame
	void Update () {
        TogglePause();
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
}
