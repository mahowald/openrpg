using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Contextual click system.

namespace ActorSystem
{

    public enum ActionContext
    {
        None, // no context
        Location, // click on a location
        Item, // click on an item (loot, door, etc)
        Cover, // click on cover
        Actor // click on another actor
    }


    public class ActionBag
    {
        private Actor owner;                                    // The Actor who holds this ActionBag
        private HashSet<IActionPrototype> actions;            // Check existence in the set
        private Dictionary<int, IActionPrototype> actionMap;    // The mapping of action to ability slot (may not contain all actions)
        private Dictionary<IActionPrototype, float> cooldowns;  // Keep track of which actions are on cooldown
        private Dictionary<IActionPrototype, int> numUses;      // Keep track of limited ammo abilities

        private Dictionary<ActionContext, IActionPrototype> contextActions;

        public ActionBag(Actor owner)
        {
            this.owner = owner;
            actionMap = new Dictionary<int, IActionPrototype>();
            cooldowns = new Dictionary<IActionPrototype, float>();
            numUses = new Dictionary<IActionPrototype, int>();
            actions = new HashSet<IActionPrototype>();
            contextActions = new Dictionary<ActionContext, IActionPrototype>();
            PopulateDefaultActions();
        }

        public IActionPrototype this[int i]
        {
            get { return actionMap[i]; }
            set {
                actionMap[i] = value;

                if(!actions.Contains(value))
                {
                    actions.Add(value);
                    cooldowns.Add(value, value.CooldownTime(owner));
                    numUses.Add(value, value.NumUses(owner));
                }
            }
        }

        public List<IActionPrototype> ActionList
        {
            get
            {
                return new List<IActionPrototype>(actions);
            }
        }

        public bool Remove(int i)
        {
            return actionMap.Remove(i);
        }

        public bool Remove(IActionPrototype actionPrototype)
        {
            if (actionMap.ContainsValue(actionPrototype))
            {
                List<int> keys = new List<int>(actionMap.Keys);
                foreach (int key in keys)
                {
                    var val = actionMap[key];
                    if (val == actionPrototype)
                    {
                        actionMap.Remove(key);
                    }
                }
            }
            cooldowns.Remove(actionPrototype);
            numUses.Remove(actionPrototype);

            return actions.Remove(actionPrototype);
        }

        public bool Add(IActionPrototype actionPrototype)
        {
            cooldowns.Add(actionPrototype, 0f); // initialize at zero
            numUses.Add(actionPrototype, actionPrototype.NumUses(owner));
            return actions.Add(actionPrototype);
        }

        public void Update(float deltaTime) // Updates the cooldowns
        {
            UpdateCooldowns(deltaTime);
        }

        private void UpdateCooldowns(float deltaTime)
        {
            foreach(IActionPrototype prototype in actions)
            {
                float currentTime = cooldowns[prototype];
                if (currentTime > 0f)
                    cooldowns[prototype] -= deltaTime;
                else
                    cooldowns[prototype] = 0f;
            }
        }

        public void ResetCooldowns()
        {
            foreach(IActionPrototype prototype in actions)
            {
                cooldowns[prototype] = 0f;
            }
        }

        public void ResetUses()
        {
            foreach(IActionPrototype prototype in actions)
            {
                numUses[prototype] = prototype.NumUses(owner);
            }
        }

        public void ResetActionPrototypes()
        {
            ResetCooldowns();
            ResetUses();
        }

        public bool Available(IActionPrototype prototype)
        {
            if (!actions.Contains(prototype))
                return false;
            if (!prototype.Allowed(owner))
                return false;
            if (cooldowns[prototype] > 0f)
                return false;
            if (numUses[prototype] == 0)
                return false;
            return true;
        }

        public IAction Instantiate<T>(IActionPrototype prototype, T target)
        {
            if(!Available(prototype))
                return null;
            if (!prototype.ValidTarget<T>(target))
                return null;
            IAction action = prototype.Instantiate<T>(owner, target);

            var localprototype = prototype;

            System.Action callback = () =>
            {
                cooldowns[localprototype] = prototype.CooldownTime(owner);
                if (numUses[localprototype] > 0)
                {
                    numUses[localprototype] -= 1;
                }
            };

            action.Callback = callback;
            return action;
        }

        private void PopulateDefaultActions()
        {
            // attack prototype
            var attackPrototype = new SingleTargetDamageActionPrototype();
            attackPrototype.Name = "Attack";
            attackPrototype.Cooldown = new Expression("0");
            attackPrototype.Cost = new Dictionary<string, Expression>();
            attackPrototype.Damage = new Dictionary<string, Expression>(); //  { { "health", new Expression("5") } };
            attackPrototype.Range = new Expression("1");
            attackPrototype.Animation = ActionAnimation.BasicAttack;

            // shoot prototype
            var shootPrototype = new ProjectileActionPrototype();
            shootPrototype.Name = "Fire";
            shootPrototype.Cooldown = new Expression("10");
            shootPrototype.Cost = new Dictionary<string, Expression>();
            shootPrototype.Range = new Expression("20");
            ProjectileData shootData = new ProjectileData();
            shootData.actionPrototype = new SingleTargetDamageActionPrototype();
            shootData.speed = 5f;
            shootData.affectedByGravity = true;
            shootData.ignoreCollisions = false;
            shootPrototype.ProjectileData = shootData;

            // move prototype
            var movePrototype = new LocatableEmptyActionPrototype();
            movePrototype.Name = "Move";

            this.Add(movePrototype);
            this.Add(attackPrototype);
            this.Add(shootPrototype);

            // Populate the default actions.
            this.contextActions.Add(ActionContext.Location, movePrototype);
            this.contextActions.Add(ActionContext.Actor, attackPrototype); 
            // Let's not think too hard about what it means that our default action for another character is to attack them.
        }

        public IActionPrototype ContextAction(ActionContext actionContext)
        {
            return contextActions[actionContext];
        }

    }

}

