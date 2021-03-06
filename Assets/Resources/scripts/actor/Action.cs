﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System;

namespace ActorSystem
{
    // Actions are performed by actors
    // All actions must have a source and a target
    // The target can be of a generic type, though 
    // (e.g., another Actor, the same Actor, a vector, ..)

    public enum ActionAnimation { None, BasicAttack };


    /// <summary>
    /// The interface for an Action.
    /// </summary>
    public interface IAction
    {
        void DoAction();
        Vector3 TargetPosition { get; }
        float Range { get; } // the range of the action
        ActionAnimation Animation { get; } // the animation to play
        System.Action Callback { get; set; } // callback function to execute during DoAction()
    }

    public interface IAction<T> : IAction
    {
        T Target { get; } // the target of the action
        Actor Source { get; } // who is performing the action
    }

    public abstract class Action<T> : IAction<T>
    {
        private T target; // who/what the action is done to
        private Actor source; // who is doing the action
        private System.Action callback = null;

        protected ActionAnimation animation = ActionAnimation.None;


        public T Target
        {
            get { return target; }
        }
        public Actor Source
        {
            get { return source; }
        }

        public Action(Actor source, T target)
        {
            this.target = target;
            this.source = source;
        }

        public Action(Actor source, T target, ActionAnimation animation)
        {
            this.target = target;
            this.source = source;
            this.animation = animation;
        }

        public ActionAnimation Animation
        {
            get { return animation; }
            set { animation = value;  }
        }
        abstract public float Range
        {
            get;
        }

        virtual public Vector3 TargetPosition
        {
            get { return Source.Position; }
        }

        // Perform the action
        virtual public void DoAction()
        {
            if (Callback != null)
                Callback();
        }

        virtual public System.Action Callback
        {
            get { return callback; }
            set { callback = value;  }
        }
    }

    // Action definitions (in YAML) get deserialized into
    // ActionPrototypes. An action prototype then produces the actual action.
    public interface IActionPrototype
    {
        string Name { get; set; }
        IActionPrototype Deserialize(string input); // Deserialize the prototype from a (YAML) string
        IAction Instantiate<T>(Actor source, T target); // Create an actual Action from the prototype
        bool Allowed(Actor source); // check if the actor is allowed to perform this action
        bool ValidTarget<T>(T target); // Can the Actor perform this action on a target?
        int NumUses(Actor source); // How many times can this action be performed? (-1 = infinite)
        float CooldownTime(Actor source); // What is the starting cooldown (in seconds) for this action?
    }

    public class EmptyActionPrototype : Utility.SerializableElement, IActionPrototype
    {
        public virtual IAction Instantiate<T>(Actor source, T target)
        {
            return new EmptyAction<T>(source, target);
        }

        public IActionPrototype Deserialize(string s)
        {
            return new EmptyActionPrototype();
        }

        public bool Allowed(Actor source)
        {
            return true;
        }

        public bool ValidTarget<T>(T target)
        {
            return true;
        }

        public int NumUses(Actor source)
        {
            return -1;
        }

        public float CooldownTime(Actor source)
        {
            return 0f;
        }
    }

    public class EmptyAction<T> : Action<T>, IAction<T>
    {
        public EmptyAction(Actor source, T target) : base(source, target)
        {
        }

        public override float Range
        {
            get { return 0f; }
        }

        public override void DoAction()
        {
            base.DoAction();
            return;
        }
    }

    public class LocatableEmptyActionPrototype : Utility.SerializableElement, IActionPrototype
    {
        
        public LocatableEmptyActionPrototype() { }

        public virtual IAction Instantiate<T>(Actor source, T target)
        {
            if(target is ILocatable)
                return new LocatableEmptyAction(source, target as ILocatable);
            else
            {
                throw new Exception("LocatableEmptyActionPrototype: Target is not ILocatable");
            }
        }

        public IActionPrototype Deserialize(string s)
        {
            return new LocatableEmptyActionPrototype();
        }

        public bool Allowed(Actor source)
        {
            return true;
        }

        public bool ValidTarget<T>(T target)
        {
            return (target is ILocatable);
        }

        public float CooldownTime(Actor source)
        {
            return 0;
        }

        public int NumUses(Actor source)
        {
            return -1;
        }
    }

    public class LocatableEmptyAction : EmptyAction<ILocatable>
    {
        public LocatableEmptyAction(Actor source, ILocatable target) : base(source, target)
        {
        }

        public override Vector3 TargetPosition
        {
            get
            {
                return Target.Position;
            }
        }
    }

    public class CombatActionPrototype : Utility.SerializableElement, IActionPrototype
    {
        // The animation to play when doing the action
        private ActionAnimation animation;
        [YamlMember(Alias = "animation")]
        public ActionAnimation Animation
        {
            get { return animation; }
            set { animation = value; }
        }

        // The recharge time between uses.
        private Expression cooldown;
        [YamlMember(Alias ="cooldown")]
        public Expression Cooldown
        {
            get { return cooldown; }
            set { cooldown = value; }
        }

        // The effects associated with this action, and their duration.
        private Dictionary<Effect, Expression> effects;

        [YamlMember(Alias ="effects")]
        public Dictionary<Effect, Expression> Effects
        {
            get { return effects; }
            set { effects = value; }
        }

        // The cost of this action to the source
        private Dictionary<string, Expression> cost;
        
        [YamlMember(Alias ="cost")]
        public Dictionary<string, Expression> Cost
        {
            get { return cost; }
            set { cost = value; }
        }

        // Whether or not this action is a ranged action
        private bool ranged;
        [YamlMember(Alias ="ranged")]
        public bool Ranged
        {
            get { return ranged; }
            set { ranged = value; }
        }

        // The range of the action (ignored if ranged=false)
        private Expression range;
        [YamlMember(Alias ="range")]
        public Expression Range
        {
            get { return range; }
            set { range = value; }
        }

        // chance of success (e.g., chance to hit for an attack)
        private Expression successChance;
        [YamlMember(Alias ="success_chance")]
        public Expression SuccessChance
        {
            get { return successChance; }
            set { successChance = value; }
        }

        // chance of critical success (e.g. critical hit)
        private Expression criticalChance;
        [YamlMember(Alias = "critical_chance")]
        public Expression CriticalChance
        {
            get { return criticalChance; }
            set { criticalChance = value; }
        }

        // special effects to apply when critical success
        private Dictionary<Effect, Expression> criticalEffects;
        [YamlMember(Alias ="critical_effects")]
        public Dictionary<Effect, Expression> CriticalEffects
        {
            get { return criticalEffects; }
            set { criticalEffects = value; }
        }

        public CombatActionPrototype()
        {
            cooldown = new Expression("0");
            effects = new Dictionary<Effect, Expression>();
            cost = new Dictionary<string, Expression>();
            successChance = new Expression("0");
            criticalChance = new Expression("0");
            criticalEffects = new Dictionary<Effect, Expression>();
            ranged = false;
            range = new Expression("0");
        }

        public virtual IAction Instantiate<T>(Actor source, T target)
        {
            return new CombatAction<T>(source, target, animation, cooldown, effects, cost, successChance, criticalChance, criticalEffects, ranged, range);
        }

        public IActionPrototype Deserialize(string s)
        {
            return new CombatActionPrototype();
        }

        public virtual bool ValidTarget<T>(T target)
        {
            return true;
        }

        // Check whether an action can be performed.
        public bool Allowed(Actor source)
        {
            Dictionary<string, float> variables = new Dictionary<string, float>();
            Attributes sourceAtts = source.attributes;
            foreach (string key in sourceAtts.attributes.Keys)
            {
                string newname = "source." + key;
                float value = sourceAtts[key];
                variables.Add(newname, value);
            }

            foreach (string key in cost.Keys)
            {
                float value = sourceAtts[key] - cost[key].Evaluate(variables);
                if (value < 0)
                    return false;
            }
            return true;
        }

        public int NumUses(Actor source)
        {
            return -1;
        }

        public float CooldownTime(Actor source)
        {
            Dictionary<string, float> variables = new Dictionary<string, float>();
            Attributes sourceAtts = source.attributes;
            foreach (string key in sourceAtts.attributes.Keys)
            {
                string newname = "source." + key;
                float value = sourceAtts[key];
                variables.Add(newname, value);
            }
            return cooldown.Evaluate(variables);
        }


    }

    public class CombatAction<T> : Action<T>, IAction<T>
    {
        protected Expression cooldown;
        protected Dictionary<Effect, Expression> effects;
        protected Dictionary<string, Expression> cost;
        protected Expression successChance;
        protected Expression criticalChance;
        protected Dictionary<Effect, Expression> criticalEffects;
        protected Dictionary<string, float> variables;
        protected Expression range;
        protected bool ranged;

        private System.Random rand;

        public CombatAction(Actor source, T target, ActionAnimation animation, Expression cooldown, Dictionary<Effect, Expression> effects, 
            Dictionary<string, Expression> cost, Expression successChance, Expression criticalChance, Dictionary<Effect, Expression> criticalEffects,
            bool ranged, Expression range) : base(source, target, animation)
        {
            this.cooldown = cooldown;
            this.effects = effects;
            this.cost = cost;
            this.successChance = successChance;
            this.criticalChance = criticalChance;
            this.criticalEffects = criticalEffects;
            this.ranged = ranged;
            this.range = range;
            variables = new Dictionary<string, float>();
            UpdateSourceVariables();
            rand = new System.Random();
        }

        private bool firstRoll = true;
        private bool firstCritRoll = true;
        private bool isSuccess;
        private bool isCriticalSuccess;

        // should only do this roll once, so that 
        // derived classes can still check the same roll
        protected bool IsSuccess
        {
            get
            {
                if(firstRoll){
                    float sroll = rand.Next(0, 100);
                    float fsuccessChance = successChance.Evaluate(variables);
                    isSuccess = (fsuccessChance > sroll);
                    firstRoll = false;
                    return isSuccess;
                }
                else
                    return isSuccess;
            }
        }

        protected bool IsCriticalSuccess
        {
            get
            {
                if (firstCritRoll)
                {
                    float sroll = rand.Next(0, 100);
                    float fcritChance = criticalChance.Evaluate(variables);
                    isCriticalSuccess = (fcritChance > sroll);
                    firstCritRoll = false;
                    return isCriticalSuccess;
                }
                else
                    return isCriticalSuccess;
            }
        }

        public override float Range
        {
            get
            {
                return range.Evaluate(variables);
            }
        }

        private void UpdateSourceVariables()
        {
            Attributes sourceAtts = this.Source.attributes;
            foreach(string key in sourceAtts.attributes.Keys)
            {
                string newname = "source." + key;
                float value = sourceAtts[key];
                variables.Add(newname, value);
            }
        }

        public virtual ActionData SourceActionData()
        {
            ActionData actionData = new ActionData();
            actionData.attributeModifier = new Dictionary<string, float>();
            actionData.bypassResistance = true;
            actionData.ranged = ranged;
            actionData.range = range.Evaluate(variables);

            foreach (string attribute in cost.Keys)
            {
                actionData.attributeModifier.Add(attribute, -1 * cost[attribute].Evaluate(variables));
            }
            return actionData;
        }

        public override void DoAction()
        {
            base.DoAction();
            var actionData = this.SourceActionData();
            Source.HandleAction(actionData);
        }
    }

    public class SingleTargetDamageActionPrototype : CombatActionPrototype, IActionPrototype
    {
        private Dictionary<string, Expression> damage;
        [YamlMember(Alias ="damage")]
        public Dictionary<string, Expression> Damage
        {
            get { return damage; }
            set { damage = value; }
        }
        private Expression criticalBonus;
        [YamlMember(Alias = "critical_multiplier")]
        public Expression CriticalBonus
        {
            get { return criticalBonus; }
            set { criticalBonus = value; }
        }

        public SingleTargetDamageActionPrototype() : base()
        {
            damage = new Dictionary<string, Expression>();
        }

        public override IAction Instantiate<T>(Actor source, T target)
        {
            Actor actor = target as Actor;
            if(actor != null)
            {
                return new SingleTargetDamageAction(source, actor, ActionAnimation.BasicAttack, Damage,
                    Cooldown, Effects, Cost, SuccessChance, CriticalChance, CriticalBonus, CriticalEffects, Ranged, Range);
            }
            else
            {
                throw new Exception("SingleTargetDamageActionPrototype: Target is not an Actor");
            }
        }

        public override bool ValidTarget<T>(T target)
        {
            return target is Actor;
        }

    }

    public class SingleTargetDamageAction : CombatAction<Actor>, IAction<Actor>
    {
        protected Dictionary<string, Expression> damage;
        protected Expression criticalBonus;

        public SingleTargetDamageAction(Actor source, Actor target, ActionAnimation animation, Dictionary<string, Expression> damage, Expression cooldown, 
            Dictionary<Effect, Expression> effects, Dictionary<string, Expression> cost, Expression successChance, 
            Expression criticalChance, Expression criticalBonus, Dictionary<Effect, Expression> criticalEffects, bool ranged, Expression range) 
            : base(source, target, animation, cooldown, effects, cost, successChance, criticalChance, criticalEffects, ranged, range)
        {
            this.damage = damage;
            this.criticalBonus = criticalBonus;
            UpdateTargetVariables();
        }

        private void UpdateTargetVariables()
        {
            Attributes targetAtts = this.Target.attributes;
            foreach (string key in targetAtts.attributes.Keys)
            {
                string newname = "target." + key;
                float value = targetAtts[key];
                variables.Add(newname, value);
            }
        }

        public override Vector3 TargetPosition
        {
            get { return Target.Position; }
        }

        public virtual ActionData GenerateTargetActionData()
        {
            bool success = IsSuccess;
            bool crit = IsCriticalSuccess;
            ActionData actionData = new ActionData();
            actionData.bypassResistance = false; // attacks should use resistances
            if (success) // if the attack hits
            {
                actionData.attributeModifier = new Dictionary<string, float>();
                actionData.effects = new Dictionary<Effect, float>();
                float multiplier = 1f; // TODO: expose this from game variables
                if (crit)
                    multiplier = criticalBonus.Evaluate(variables);
                foreach (string attribute in damage.Keys)
                {
                    if (actionData.attributeModifier.ContainsKey(attribute))
                        actionData.attributeModifier[attribute] += -1 * multiplier * damage[attribute].Evaluate(variables);
                    else
                        actionData.attributeModifier.Add(attribute, -1 * multiplier * damage[attribute].Evaluate(variables));
                }
                foreach (Effect effect in effects.Keys)
                {
                    if (actionData.effects.ContainsKey(effect))
                        actionData.effects[effect] = effects[effect].Evaluate(variables);
                    else
                        actionData.effects.Add(effect, effects[effect].Evaluate(variables));
                }
                if (crit)
                {
                    foreach (Effect effect in criticalEffects.Keys)
                    {
                        if (actionData.effects.ContainsKey(effect))
                            actionData.effects[effect] = criticalEffects[effect].Evaluate(variables);
                        else
                            actionData.effects.Add(effect, criticalEffects[effect].Evaluate(variables));
                    }
                }
            }
            else if (crit) // critical hits always do some damage, e.g., "grazed" or "glancing hit"
            {
                actionData.attributeModifier = new Dictionary<string, float>();
                foreach (string attribute in damage.Keys)
                {
                    // TODO: Should we apply a multiplier for grazing hits?
                    if (actionData.attributeModifier.ContainsKey(attribute))
                        actionData.attributeModifier[attribute] += -1 * damage[attribute].Evaluate(variables);
                    else
                        actionData.attributeModifier.Add(attribute, -1 * damage[attribute].Evaluate(variables));
                }
            }

            return actionData;
        }

        public override void DoAction()
        {
            base.DoAction();
            ActionData actionData = GenerateTargetActionData();
            Target.HandleAction(actionData);
        }

    }

    public class ProjectileActionPrototype : CombatActionPrototype, IActionPrototype
    {
        private ProjectileData projectileData;
        [YamlMember(Alias = "projectile_data")]
        public ProjectileData ProjectileData
        {
            get { return projectileData;}
            set { projectileData = value;}
        }

        public override IAction Instantiate<T>(Actor source, T target)
        {
            ILocatable tar = target as ILocatable;
            if(tar != null)
            {
                return new ProjectileAction(source, tar, ActionAnimation.BasicAttack, Cooldown, Effects, Cost,
                    SuccessChance, CriticalChance, CriticalEffects, Range, ProjectileData);
            }
            else
            {
                throw new Exception("ProjectileActionPrototype: Target does not implement ILocatable.");
            }
        }
    }

    /// <summary>
    /// Action that creates a projectile directed to the target
    /// </summary>
    public class ProjectileAction : CombatAction<ILocatable>, IAction<ILocatable>
    {
        protected ProjectileData projectileData;
        public ProjectileAction(Actor source, ILocatable target, ActionAnimation animation, Expression cooldown, Dictionary<Effect, Expression> effects,
            Dictionary<string, Expression> cost, Expression successChance, Expression criticalChance, Dictionary<Effect, Expression> criticalEffects,
            Expression range, ProjectileData projectileData)
            : base(source, target, animation, cooldown, effects, cost, successChance, criticalChance, criticalEffects, ranged:true, range:range)
        {
            this.projectileData = projectileData;
        }

        override public void DoAction() 
        {
            base.DoAction();
            ActionData sourceActionData = SourceActionData();
            Source.HandleAction(sourceActionData);
            Projectile.Create(projectileData, Source, Target);
        }
    }
}
