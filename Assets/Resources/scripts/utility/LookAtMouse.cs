using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LookAtMouse : MonoBehaviour {

    Geometry.Arc arc = new Geometry.Arc(5f, 30f, Vector2.zero, Vector2.up);

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        GetMousePosition();
        transform.rotation = Quaternion.LookRotation(mousePosition - transform.position, Vector3.up);

        arc.direction = new Vector2(transform.forward.x, transform.forward.z);
        arc.origin = new Vector2(transform.position.x, transform.position.z);

        List<ActorSystem.Actor> actors = Geometry.GetActorsInArc(arc, a => a.name != "hero-f");
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
}
