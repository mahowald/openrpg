using UnityEngine;
using System.Collections;

public class DynamicButton : MonoBehaviour {

    public UnityEngine.UI.Text text;
    public UnityEngine.UI.Button button;

    public string Text
    {
        get
        {
            return text.text;
        }
        set
        {
            text.text = value;
        }
    }
    
    public RectTransform RectTransform
    {
        get
        {
            return this.GetComponent<RectTransform>();
        }
    }

    public void AddListener(UnityEngine.Events.UnityAction action)
    {
        button.onClick.AddListener(action);
    }

    public void RemoveListener(UnityEngine.Events.UnityAction action)
    {
        button.onClick.RemoveListener(action);
    }

    public void RemoveAllListeners()
    {
        button.onClick.RemoveAllListeners();
    }
}
