using UnityEngine;
using System.Collections;

public class ClickToMove : MonoBehaviour
{

    public Actor actor;
    Vector3 targetPosition;

    // Use this for initialization
    void Start()
    {
        targetPosition = transform.position;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                targetPosition = hit.point;
                actor.Destination = targetPosition;
            }
        }
    }
}
