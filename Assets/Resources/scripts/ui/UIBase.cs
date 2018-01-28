using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UserInterface
{
    public class UIBase : MonoBehaviour
    {

        public DynamicButton genericButton;

        private Vector3 pointContext;
        private ActorSystem.Actor actorContext;

        public Camera mycamera;
        public RectTransform uiPanel;

        public RectTransform moveButton;
        public RectTransform attackButton;
        public RectTransform swapButton;
        public RectTransform abilityButton;

        private ActorSystem.LocatableEmptyActionPrototype movePrototype;
        private ActorSystem.SingleTargetDamageActionPrototype attackPrototype;

        private List<DynamicButton> buttonPool;

        // Use this for initialization
        void Start()
        {
            moveButton.gameObject.SetActive(false);
            abilityButton.gameObject.SetActive(false);
            attackButton.gameObject.SetActive(false);
            swapButton.gameObject.SetActive(false);
            uiPanel.gameObject.SetActive(false);

            buttonPool = new List<DynamicButton>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void Relocate(Vector3 dest, Vector3 offset)
        {
            uiPanel.anchoredPosition = mycamera.WorldToScreenPoint(dest + offset);
        }

        void ContextClick(ActorSystem.Actor actor)
        {
            actorContext = actor;
            GenerateButtonPool(ActorSystem.ActionContext.Actor, actor.Position);
        }

        void ContextClick(Vector3 location)
        {
            actorContext = null;
            pointContext = location;
            GenerateButtonPool(ActorSystem.ActionContext.Location, location);
        }

        void GenerateButtonPool(ActorSystem.ActionContext context, Vector3 location)
        {
            ClearButtonPool();
            // List<ActorSystem.IActionPrototype> actions = GameController.PlayerActor.actionMapper[context];
            List<ActorSystem.IActionPrototype> actions = GameController.PlayerActor.actionBag.ActionList;

            ILocatable target = null;
            switch (context)
            {
                case ActorSystem.ActionContext.Actor:
                    target = actorContext;
                    break;
                case ActorSystem.ActionContext.Location:
                    Geometry.Locatable locator = new Geometry.Locatable();
                    locator.Position = location;
                    target = locator;
                    break;
            }

            // Create buttons from each action
            foreach(ActorSystem.IActionPrototype protoaction in actions)
            {
                //if (protoaction.Allowed(GameController.PlayerActor))
                if (GameController.PlayerActor.actionBag.Available(protoaction))
                {

                    DynamicButton d_actionbutton = Instantiate(genericButton, uiPanel) as DynamicButton;
                    d_actionbutton.Text = protoaction.Name;
                    var local_protoaction = protoaction; // need to make a local copy, otherwise every button does the last action available
                    UnityEngine.Events.UnityAction btn_fcn = () =>
                    {
                        ActorSystem.IAction action = null;
                        action = GameController.PlayerActor.actionBag.Instantiate(local_protoaction, target); // local_protoaction.Instantiate(GameController.PlayerActor, target);
                        EventManager.TriggerEvent("DoAction", GameController.PlayerActor, action);
                        uiPanel.gameObject.SetActive(false);
                        ClearButtonPool();
                    };
                    d_actionbutton.AddListener(btn_fcn);
                    buttonPool.Add(d_actionbutton);
                }
                else
                {
                    DynamicButton d_actionbutton = Instantiate(genericButton, uiPanel) as DynamicButton;
                    d_actionbutton.Text = "(" + protoaction.Name + ")";
                    buttonPool.Add(d_actionbutton);
                }
            }
            
            // add the swap button
            if(context == ActorSystem.ActionContext.Actor)
            {
                DynamicButton d_swapbutton = Instantiate(genericButton, uiPanel) as DynamicButton;
                d_swapbutton.Text = "Swap";
                d_swapbutton.AddListener(Swap);
                buttonPool.Add(d_swapbutton);
            }

            Vector3 offset = Vector3.zero;
            if (context == ActorSystem.ActionContext.Actor)
                offset = new Vector3(0f, 2f, 0f);
            
            uiPanel.gameObject.SetActive(true);
            Relocate(location, offset);

            ButtonPositioner.layout(buttonPool);
        }

        void ActorRightSelected(ActorSystem.Actor actor)
        {
            actorContext = actor;
            Attack();
        }

        void PointRightSelected(Vector3 point)
        {
            pointContext = point;
            Move();
        }

        /**
        void PointSelected(Vector3 point)
        {
            ClearButtonPool(); // otherwise buttons may multiply

            DynamicButton d_movebutton = Instantiate(genericButton, uiPanel) as DynamicButton;
            d_movebutton.Text = "Go Here";
            d_movebutton.AddListener(Move);
            buttonPool.Add(d_movebutton);

            DynamicButton d_abilitybutton = Instantiate(genericButton, uiPanel) as DynamicButton;
            d_abilitybutton.Text = "Ability 1";
            // TODO: Add listener...
            buttonPool.Add(d_abilitybutton);
            
            pointContext = point;
            actorContext = null;
            Vector3 offset = Vector3.zero;
            uiPanel.gameObject.SetActive(true);
            Relocate(point, offset);

            ButtonPositioner.layout(buttonPool);

        }
    **/

        // Subscribe/Unsubscribe to our events
        public void OnEnable()
        {
            EventManager.StartListening<ActorSystem.Actor>("ContextClick", ContextClick);
            EventManager.StartListening<Vector3>("ContextClick", ContextClick);
            EventManager.StartListening<ActorSystem.Actor>("ContextRightClick", ActorRightSelected);
            EventManager.StartListening<Vector3>("ContextRightClick", PointRightSelected);
        }
        public void OnDisable()
        {
            EventManager.StopListening<ActorSystem.Actor>("ContextClick", ContextClick);
            EventManager.StopListening<Vector3>("ContextClick", ContextClick);
            EventManager.StopListening<ActorSystem.Actor>("ContextRightClick", ActorRightSelected);
            EventManager.StopListening<Vector3>("ContextRightClick", PointRightSelected);
        }

        // buttons
        public void Swap()
        {
            if (actorContext != null)
            {
                EventManager.TriggerEvent("ActorClicked", actorContext);
                actorContext = null;
                uiPanel.gameObject.SetActive(false);
                ClearButtonPool();
            }
        }

        // These get called by the contextual clicks.
        public void Move()
        {
            
            Geometry.Locatable target = new Geometry.Locatable();
            if (actorContext == null)
            {
                target.Position = pointContext;
            }
            else
            {
                target.Position = actorContext.Position;
            }
            target.Direction = Vector3.zero;

            // generate the action from the Actor
            var moveActionPrototype = GameController.PlayerActor.actionBag.ContextAction(ActorSystem.ActionContext.Location);
            if (GameController.PlayerActor.actionBag.Available(moveActionPrototype))
            {
                ActorSystem.IAction action = GameController.PlayerActor.actionBag.Instantiate(moveActionPrototype, target);
                EventManager.TriggerEvent("DoAction", GameController.PlayerActor, action);
            }
            
            uiPanel.gameObject.SetActive(false);
            ClearButtonPool();
        }

        public void Attack()
        {
            
            if (actorContext != null)
            {
                var attackActionPrototype = GameController.PlayerActor.actionBag.ContextAction(ActorSystem.ActionContext.Actor);

                if (GameController.PlayerActor.actionBag.Available(attackActionPrototype))
                {
                    ActorSystem.IAction action = GameController.PlayerActor.actionBag.Instantiate(attackActionPrototype, actorContext);
                    EventManager.TriggerEvent("DoAction", GameController.PlayerActor, action);
                }
                
                uiPanel.gameObject.SetActive(false);
                ClearButtonPool();
            }
        }

        public void ClearButtonPool()
        {
            foreach(DynamicButton btn in buttonPool)
            {
                Destroy(btn.gameObject);
            }
            buttonPool.Clear();
        }
    }
}
