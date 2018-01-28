using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPowerPanel : MonoBehaviour {

    RectTransform panelTransform;
    bool hidden = false;
    float duration = 1f;

    bool orthographicMode = false;
    


	// Use this for initialization
	void Start () {
        panelTransform = this.GetComponent<RectTransform>();

        Debug.Log(panelTransform.anchoredPosition);
	}
	
	// Update is called once per frame
	void Update () {
        if (orthographicMode) // always show it in ortho mode
        {
            if (hidden)
            {
                StopAllCoroutines();
                StartCoroutine(Show(this.duration));
                hidden = false;
            }
        }
        else
        {
            bool mouseover = MouseOver();
            if(mouseover && hidden)
            {
                StopAllCoroutines();
                StartCoroutine(Show(this.duration));
                hidden = false;
            }
            else if(!hidden && !mouseover)
            {
                StopAllCoroutines();
                StartCoroutine(Hide(this.duration));
                hidden = true;
            }

        }

        
	}



    private IEnumerator Hide(float duration)
    {
        float startTime = Time.time;
        while(panelTransform.anchoredPosition.y > -1*panelTransform.rect.height && Time.time - startTime < duration)
        {
            float deltaTime = Time.time - startTime;
            var anchorposition = panelTransform.anchoredPosition;
            anchorposition.y = anchorposition.y - deltaTime * panelTransform.rect.height / duration;
            panelTransform.anchoredPosition = anchorposition;
            yield return 1;
        }

        // lock the final position
        var fanchorposition = panelTransform.anchoredPosition;
        fanchorposition.y = -1 * panelTransform.rect.height;
        panelTransform.anchoredPosition = fanchorposition;
    }

    private IEnumerator Show(float duration)
    {
        float startTime = Time.time;
        while (panelTransform.anchoredPosition.y < 0 && Time.time - startTime < duration)
        {
            float deltaTime = Time.time - startTime;
            var anchorposition = panelTransform.anchoredPosition;
            anchorposition.y = anchorposition.y + deltaTime * panelTransform.rect.height / duration;
            panelTransform.anchoredPosition = anchorposition;
            yield return 1;
        }

        // lock the final position
        var fanchorposition = panelTransform.anchoredPosition;
        fanchorposition.y = 0;
        panelTransform.anchoredPosition = fanchorposition;
    }

    bool MouseOver()
    {

        if (!Cursor.visible)
            return false;

        Vector2 mousePosition = Input.mousePosition;
        if (mousePosition.y < panelTransform.rect.height && Mathf.Abs(mousePosition.x - Screen.width/2) < panelTransform.rect.width/2)
            return true;
        return false;
    }

    void OrthographicMode(bool orthographicMode)
    {
        Debug.Log("Hit this!");
        this.orthographicMode = orthographicMode;
    }


    public void OnEnable()
    {
        EventManager.StartListening<bool>("Orthographic Mode", OrthographicMode);
    }

    public void OnDisable()
    {
        EventManager.StopListening<bool>("Orthographic Mode", OrthographicMode);
    }

    /**
    public void OnEnable()
    {
        EventManager.StartListening("Pause", Pause);
        EventManager.StartListening("Unpause", Unpause);
        EventManager.StartListening<Actor>("ActorClicked", ActorClicked);
        EventManager.StartListening<Actor, IAction>("DoAction", DoActionMessage);
    }
    public void OnDisable()
    {
        EventManager.StopListening("Pause", Pause);
        EventManager.StopListening("Unpause", Unpause);
        EventManager.StopListening<Actor>("ActorClicked", ActorClicked);
        EventManager.StopListening<Actor, IAction>("DoAction", DoActionMessage);
    }
    **/

}
