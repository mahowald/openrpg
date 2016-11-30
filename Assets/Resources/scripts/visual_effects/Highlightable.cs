using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Standalone class to allow for object highlighting.
/// Note: this is built in to the Actor class. 
/// </summary>
public class Highlightable : MonoBehaviour {

    private ActorSystem.Highlighter highlighter;

	// Use this for initialization
	void Start ()
    {
        highlighter = new ActorSystem.Highlighter(this.gameObject);
	}
	
	// Update is called once per frame
	void Update () {
	    if(Input.GetKeyUp("h"))
        {
            Highlighted = !Highlighted;
        }
	}

    bool highlighted = false;
    bool Highlighted
    {
        get { return highlighter.Highlighted; }
        set { highlighter.Highlighted = value; }
    }
}
