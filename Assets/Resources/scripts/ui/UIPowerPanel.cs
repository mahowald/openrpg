﻿using System.Collections;
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
        public Image cooldownImage;
        public UIBase contextUI;


        private List<DynamicButton> buttonPool;
        private Dictionary<DynamicButton, Tuples.Tuple<ActorSystem.IActionPrototype, Image>> buttonMap;

        private ActorSystem.IActionPrototype queuedProtoAction = null;
        private ActorSystem.Actor selectedActor = null;



        // Use this for initialization
        void Start()
        {
            panelTransform = this.GetComponent<RectTransform>();
            buttonPool = new List<DynamicButton>();
            buttonMap = new Dictionary<DynamicButton, Tuples.Tuple<ActorSystem.IActionPrototype, Image>>();
        }

        // Update is called once per frame
        void Update()
        {
            if (selectedActor == null)
            {
                selectedActor = GameController.PlayerActor;
                ReloadButtonPool();
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

            if (!hidden)
                RefreshCooldowns();


        }

        private void RefreshCooldowns()
        {
            foreach(var item in buttonMap)
            {
                var pwr = item.Value.Item1;
                var img = item.Value.Item2;

                float cooldown = selectedActor.actionBag.Cooldown(pwr);
                if (cooldown <= 0f)
                    img.fillAmount = 0f;
                else
                {

                    float cooldownPrcnt = selectedActor.actionBag.Cooldown(pwr) / pwr.CooldownTime(selectedActor);
                    img.fillAmount = cooldownPrcnt;
                }
            }
        }

        private void ReloadButtonPool()
        {
            ClearButtonPool();

            List<ActorSystem.IActionPrototype> actions = selectedActor.actionBag.ActionList;

            foreach (ActorSystem.IActionPrototype protoaction in actions)
            {
                if (selectedActor.actionBag.Available(protoaction))
                {
                    DynamicButton d_actionbutton = Instantiate(genericButton, panelTransform) as DynamicButton;
                    Image d_cooldownimg = Instantiate(cooldownImage, d_actionbutton.RectTransform) as Image;
                    d_cooldownimg.rectTransform.sizeDelta = new Vector2(d_actionbutton.RectTransform.rect.width, d_actionbutton.RectTransform.rect.height);
                    d_actionbutton.Text = protoaction.Name;

                    var local_protoaction = protoaction;
                    UnityEngine.Events.UnityAction btn_fcn = () =>
                    {
                        this.queuedProtoAction = local_protoaction;
                        this.contextUI.powerButtonSelected = true;
                    };
                    
                    d_actionbutton.AddListener(btn_fcn);
                    buttonPool.Add(d_actionbutton);
                    buttonMap.Add(d_actionbutton, new Tuples.Tuple<ActorSystem.IActionPrototype, Image>(local_protoaction, d_cooldownimg));
                }
                else // Does this code path ever get hit?
                {
                    DynamicButton d_actionbutton = Instantiate(genericButton, panelTransform) as DynamicButton;
                    Image d_cooldownimg = Instantiate(cooldownImage, d_actionbutton.RectTransform) as Image;
                    d_cooldownimg.rectTransform.sizeDelta = new Vector2(d_actionbutton.RectTransform.rect.width, d_actionbutton.RectTransform.rect.height);
                    d_actionbutton.Text = "(" + protoaction.Name + ")";
                    buttonPool.Add(d_actionbutton);
                    buttonMap.Add(d_actionbutton, new Tuples.Tuple<ActorSystem.IActionPrototype, Image>(protoaction, d_cooldownimg));
                }
            }
            LayoutButtonPool();
        }

        private void ClearButtonPool()
        {
            foreach (DynamicButton btn in buttonPool)
            {
                Destroy(buttonMap[btn].Item2.gameObject);
                Destroy(btn.gameObject);
            }
            buttonPool.Clear();
            buttonMap.Clear();
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
            return DelayToNextFrame(func, 1);
        }

        IEnumerator DelayToNextFrame(System.Action func, int frames)
        {
            for(int i = 0; i < frames; i++)
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
                    StartCoroutine(DelayToNextFrame(() => { contextUI.powerButtonSelected = false; }));
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
                    StartCoroutine(DelayToNextFrame(() => { contextUI.powerButtonSelected = false; }));
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
            ReloadButtonPool();
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

    }
}
