using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UserInterface
{

    public class UIPowerPanel : MonoBehaviour
    {

        RectTransform panelTransform;
        bool hidden = false;
        float duration = 1f;

        bool orthographicMode = false;

        public DynamicButton genericButton;
        public UIBase contextUI;


        private List<DynamicButton> buttonPool;

        private ActorSystem.IActionPrototype queuedProtoAction = null;
        private ActorSystem.Actor selectedActor = null;


        // Use this for initialization
        void Start()
        {
            panelTransform = this.GetComponent<RectTransform>();
            buttonPool = new List<DynamicButton>();
        }

        // Update is called once per frame
        void Update()
        {
            if (selectedActor == null)
            {
                selectedActor = GameController.PlayerActor;
                RefreshButtonPool();
            }
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
                if (mouseover && hidden)
                {
                    StopAllCoroutines();
                    StartCoroutine(Show(this.duration));
                    hidden = false;
                }
                else if (!hidden && !mouseover)
                {
                    StopAllCoroutines();
                    StartCoroutine(Hide(this.duration));
                    hidden = true;
                }

            }


        }

        private void RefreshButtonPool()
        {
            ClearButtonPool();

            List<ActorSystem.IActionPrototype> actions = selectedActor.actionBag.ActionList;

            foreach (ActorSystem.IActionPrototype protoaction in actions)
            {
                if (selectedActor.actionBag.Available(protoaction))
                {
                    DynamicButton d_actionbutton = Instantiate(genericButton, panelTransform) as DynamicButton;
                    d_actionbutton.Text = protoaction.Name;

                    var local_protoaction = protoaction;
                    UnityEngine.Events.UnityAction btn_fcn = () =>
                    {
                        this.queuedProtoAction = local_protoaction;
                        this.contextUI.powerButtonSelected = true;
                    };
                    
                    d_actionbutton.AddListener(btn_fcn);
                    buttonPool.Add(d_actionbutton);
                }
                else
                {
                    DynamicButton d_actionbutton = Instantiate(genericButton, panelTransform) as DynamicButton;
                    d_actionbutton.Text = "(" + protoaction.Name + ")";
                    buttonPool.Add(d_actionbutton);
                }
            }
            LayoutButtonPool();
        }

        private void ClearButtonPool()
        {
            foreach (DynamicButton btn in buttonPool)
                Destroy(btn.gameObject);
            buttonPool.Clear();
        }

        private void LayoutButtonPool()
        {
            float padding = 5f;
            float maxwidth = 0f;
            foreach(DynamicButton btn in buttonPool)
            {
                float btnwidth = btn.RectTransform.rect.width;
                maxwidth += btnwidth + padding;
            }
            if (maxwidth > 0)
                maxwidth -= padding;

            float offset = -1f * maxwidth / 2f;
            
            foreach (DynamicButton btn in buttonPool)
            {
                RectTransform rect = btn.RectTransform;
                rect.anchoredPosition = new Vector2(offset, -1f*rect.rect.height/2f);
                offset += rect.rect.width + padding;
            }

            panelTransform.sizeDelta = new Vector2(2f * padding + maxwidth, panelTransform.sizeDelta.y);
        }


        private IEnumerator Hide(float duration)
        {
            float startTime = Time.time;
            while (panelTransform.anchoredPosition.y > -1 * panelTransform.rect.height && Time.time - startTime < duration)
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
            float deltaTime = 0f;
            while (panelTransform.anchoredPosition.y < 0 - deltaTime * panelTransform.rect.height / duration && Time.time - startTime < duration)
            {
                deltaTime = Time.time - startTime;
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
            if (mousePosition.y < panelTransform.rect.height && Mathf.Abs(mousePosition.x - Screen.width / 2) < panelTransform.rect.width / 2)
                return true;
            return false;
        }

        void OrthographicMode(bool orthographicMode)
        {
            this.orthographicMode = orthographicMode;
        }

        void ClearSelectedButton<T>(T target)
        {
            this.queuedProtoAction = null;
            this.selectedActor = null;
            StartCoroutine(DelayToNextFrame(() => { contextUI.powerButtonSelected = false;  }));
        }

        IEnumerator DelayToNextFrame(System.Action func)
        {
            yield return null;
            func();
        }

        void PointSelected(Vector3 target)
        {
            if (queuedProtoAction != null && selectedActor != null && selectedActor.actionBag.Available(queuedProtoAction))
            {
                Geometry.Locatable t = new Geometry.Locatable();
                t.Position = target;
                if (this.queuedProtoAction.ValidTarget(t))
                {
                    ActorSystem.IAction action = null;
                    action = selectedActor.actionBag.Instantiate(queuedProtoAction, t);
                    EventManager.TriggerEvent("DoAction", selectedActor, action);
                    contextUI.powerButtonSelected = false;
                    queuedProtoAction = null;
                }
            }
        }

        void TargetSelected<T>(T target)
        {
            if(queuedProtoAction != null && selectedActor != null && selectedActor.actionBag.Available(queuedProtoAction))
            {
                if (queuedProtoAction.ValidTarget(target))
                {
                    ActorSystem.IAction action = null;
                    action = selectedActor.actionBag.Instantiate(queuedProtoAction, target);
                    EventManager.TriggerEvent("DoAction", selectedActor, action);
                    contextUI.powerButtonSelected = false; // TODO: set this in the next frame.
                    queuedProtoAction = null;
                }
            }
        }

        // NB we have to maintain our own tracking of the currently selected actor
        // because this event may be called BEFORE the GameController updates its selected actor.

        void Swap(ActorSystem.Actor a)
        {
            selectedActor = a;
            queuedProtoAction = null;
            RefreshButtonPool();
        }

        public void OnEnable()
        {
            EventManager.StartListening<bool>("Orthographic Mode", OrthographicMode);
            EventManager.StartListening<ActorSystem.Actor>("ContextRightClick", ClearSelectedButton); // unfortunately we have to add these for every
            EventManager.StartListening<Vector3>("ContextRightClick", ClearSelectedButton);           // possible contextual click
            EventManager.StartListening<ActorSystem.Actor>("ContextClick", TargetSelected);
            EventManager.StartListening<Vector3>("ContextClick", PointSelected);
            EventManager.StartListening<ActorSystem.Actor>("ActorClicked", Swap);
        }

        public void OnDisable()
        {
            EventManager.StopListening<bool>("Orthographic Mode", OrthographicMode);
            EventManager.StopListening<ActorSystem.Actor>("ContextRightClick", ClearSelectedButton);
            EventManager.StopListening<Vector3>("ContextRightClick", ClearSelectedButton);
            EventManager.StopListening<ActorSystem.Actor>("ContextClick", TargetSelected);
            EventManager.StopListening<Vector3>("ContextClick", PointSelected);
            EventManager.StopListening<ActorSystem.Actor>("ActorClicked", Swap);
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
}
