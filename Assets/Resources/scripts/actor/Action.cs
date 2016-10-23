using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization;
using System;

namespace Actor
{
    // Actions are performed by actors
    // All actions must have a source and a target
    // The target can be of a generic type, though 
    // (e.g., another Actor, the same Actor, a vector, ..)
    public interface IAction
    {
        void DoAction();
    }
    
    public interface IAction<T> : IAction
    {
        T Target { get; }
        Actor Source { get; }
    }
    public abstract class Action<T> : IAction<T>
    {
        private T target; // who/what the action is done to
        private Actor source; // who is doing the action
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

        // Perform the action
        abstract public void DoAction();
    }

    // Action definitions (in YAML) get deserialized into
    // ActionPrototypes. An action prototype then produces the actual action.
    public interface IActionPrototype<T>
    {
        IActionPrototype<T> Deserialize(string input); // Deserialize the prototype from a (YAML) string
        IAction<T> Instantiate(Actor source, T target); // Create an actual Action from the prototype
    }

    public class CombatActionPrototype<T> : IActionPrototype<T>
    {

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
        }

        public virtual IAction<T> Instantiate(Actor source, T target)
        {
            return new CombatAction<T>(source, target, cooldown, effects, cost, successChance, criticalChance, criticalEffects);
        }

        public IActionPrototype<T> Deserialize(string s)
        {
            return new CombatActionPrototype<T>();
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

        private System.Random rand;

        public CombatAction(Actor source, T target, Expression cooldown, Dictionary<Effect, Expression> effects, 
            Dictionary<string, Expression> cost, Expression successChance, Expression criticalChance, Dictionary<Effect, Expression> criticalEffects) : base(source, target)
        {
            this.cooldown = cooldown;
            this.effects = effects;
            this.cost = cost;
            this.successChance = successChance;
            this.criticalChance = criticalChance;
            this.criticalEffects = criticalEffects;
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
                    float fsuccessChance = successChance.Evaluate();
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
                    float fcritChance = criticalChance.Evaluate();
                    isCriticalSuccess = (fcritChance > sroll);
                    firstCritRoll = false;
                    return isCriticalSuccess;
                }
                else
                    return isCriticalSuccess;
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

        public override void DoAction()
        {
            foreach(string attribute in cost.Keys)
            {
                Source.attributes[attribute] -= cost[attribute].Evaluate(variables);
            }
        }
    }

    public class SingleTargetDamageActionPrototype : CombatActionPrototype<Actor>, IActionPrototype<Actor>
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

        public override IAction<Actor> Instantiate(Actor source, Actor target)
        {
            return new SingleTargetDamageAction(source, target, damage, 
                Cooldown, Effects, Cost, SuccessChance, CriticalChance, criticalBonus, CriticalEffects);
        }

    }

    public class SingleTargetDamageAction : CombatAction<Actor>, IAction<Actor>
    {
        protected Dictionary<string, Expression> damage;
        protected Expression criticalBonus;

        public SingleTargetDamageAction(Actor source, Actor target, Dictionary<string, Expression> damage, Expression cooldown, 
            Dictionary<Effect, Expression> effects, Dictionary<string, Expression> cost, Expression successChance, 
            Expression criticalChance, Expression criticalBonus, Dictionary<Effect, Expression> criticalEffects) 
            : base(source, target, cooldown, effects, cost, successChance, criticalChance, criticalEffects)
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

        public override void DoAction()
        {
            base.DoAction();
            bool success = IsSuccess;
            bool crit = IsCriticalSuccess;
            if(success) // if the attack hits
            {
                float multiplier = 1f;
                if (crit)
                    multiplier = criticalBonus.Evaluate(variables);
                foreach(string attribute in damage.Keys)
                {
                    Target.attributes[attribute] -= multiplier*damage[attribute].Evaluate(variables);
                }
                foreach(Effect effect in effects.Keys)
                {
                    Target.AddEffect(effect, effects[effect].Evaluate(variables));
                }
                if (crit)
                {
                    foreach(Effect effect in criticalEffects.Keys)
                    {
                        Target.AddEffect(effect, criticalEffects[effect].Evaluate(variables));
                    }
                }
            }
            else if(crit) // critical hits always do some damage, e.g., "grazed" or "glancing hit"
            {
                foreach (string attribute in damage.Keys)
                {
                    // TODO: Should we apply a multiplier for grazing hits?
                    Target.attributes[attribute] -= damage[attribute].Evaluate(variables); 
                }
            }
        }

    }

    public class ActorTransactionPrototype : IActionPrototype<Actor>
    {
        private Dictionary<string, Expression> targetDiff;
        private Dictionary<string, Expression> sourceDiff;

        [YamlMember(Alias = "target_diff")]
        public Dictionary<string, Expression> TargetDiff
        {
            get { return targetDiff; }
            set { targetDiff = value; }
        }

        [YamlMember(Alias = "source_diff")]
        public Dictionary<string, Expression> SourceDiff
        {
            get { return sourceDiff; }
            set { sourceDiff = value; }
        }

        // default constructor
        public ActorTransactionPrototype()
        {
            targetDiff = new Dictionary<string, Expression>();
            sourceDiff = new Dictionary<string, Expression>();
        }

        public ActorTransactionPrototype(Dictionary<string, Expression> targetDiff, Dictionary<string, Expression> sourceDiff)
        {
            this.targetDiff = targetDiff;
            this.sourceDiff = sourceDiff;
        }

        public virtual IActionPrototype<Actor> Deserialize(string input)
        {
            return new ActorTransactionPrototype(null, null);
        }

        public virtual IAction<Actor> Instantiate(Actor source, Actor target)
        {
            return new ActorTransaction(source, target, targetDiff, sourceDiff);
        }
    }

    // An actor transaction directly modifies the attributes of the actors involved.
    // For example, a heal spell should increase the hitpoints of the target actor,
    // but decrease the mana of the source actor.
    // A "free" action (e.g. a basic attack) might have no effect on the attributes
    // of the source actor. 
    public class ActorTransaction : Action<Actor>, IAction<Actor>
    {
        protected Dictionary<string, float> variables;
        protected Dictionary<string, Expression> targetDiff;
        protected Dictionary<string, Expression> sourceDiff;
        public ActorTransaction(Actor source, Actor target, Dictionary<string, Expression> targetDiff, Dictionary<string, Expression> sourceDiff) : base(source, target)
        {
            variables = AggregateVariables();
            this.targetDiff = targetDiff;
            this.sourceDiff = sourceDiff;
        }

        public override void DoAction()
        {
            foreach(string attribute in targetDiff.Keys)
            {
                Target.attributes[attribute] += targetDiff[attribute].Evaluate(variables);
            }
            foreach (string attribute in sourceDiff.Keys)
            {
                Source.attributes[attribute] += sourceDiff[attribute].Evaluate(variables);
            }
        }
        
        private Dictionary<string, float> AggregateVariables()
        {
            Dictionary<string, float> aggregated = new Dictionary<string, float>();
            Attributes sourceAtts = this.Source.attributes;
            Attributes targetAtts = this.Target.attributes;
            foreach(string key in sourceAtts.attributes.Keys)
            {
                string newname = "source." + key;
                float value = sourceAtts[key];
                aggregated.Add(newname, value);
            }
            foreach (string key in targetAtts.attributes.Keys)
            {
                string newname = "target." + key;
                float value = targetAtts[key];
                aggregated.Add(newname, value);
            }

            return aggregated;
        }
    }

    // Actor transactions that have a chance of failure
    public class ProbabalisticActorTransactionPrototype : ActorTransactionPrototype
    {
        private Expression sourceSuccessChance;
        private Expression targetSuccessChance;

        public ProbabalisticActorTransactionPrototype() : base()
        {
            sourceSuccessChance = new Expression("100"); // default chance is 100
            targetSuccessChance = new Expression("100"); // default chance is 100
        }

        public ProbabalisticActorTransactionPrototype(Dictionary<string, Expression> targetDiff, Dictionary<string, Expression> sourceDiff, 
            Expression targetSuccessChance, Expression sourceSuccessChance) : base(targetDiff, sourceDiff)
        {
            this.targetSuccessChance = targetSuccessChance;
            this.sourceSuccessChance = sourceSuccessChance;
        }

        [YamlMember(Alias = "source_chance")]
        public Expression SourceSuccessChance
        {
            get { return sourceSuccessChance; }
            set { sourceSuccessChance = value; }
        }

        [YamlMember(Alias = "target_chance")]
        public Expression TargetSuccessChance
        {
            get { return targetSuccessChance; }
            set { targetSuccessChance = value; }
        }
        
        public override IAction<Actor> Instantiate(Actor source, Actor target)
        {
            return new ProbabalisticActorTransaction(source, target, TargetDiff, SourceDiff, targetSuccessChance, sourceSuccessChance);
        }

    }
    
    // Transactions that have a chance of failure
    public class ProbabalisticActorTransaction : ActorTransaction
    {
        protected Expression sourceSuccessChance;
        protected Expression targetSuccessChance;
        public ProbabalisticActorTransaction(Actor source, Actor target, Dictionary<string, Expression> targetDiff, Dictionary<string, Expression> sourceDiff, 
            Expression targetSuccessChance, Expression sourceSuccessChance) : base(source, target, targetDiff, sourceDiff)
        {
            this.sourceSuccessChance = sourceSuccessChance;
            this.targetSuccessChance = targetSuccessChance;
        }

        public override void DoAction()
        {
            System.Random rand = new System.Random();
            float sourceSuccess = sourceSuccessChance.Evaluate(variables);
            float targetSuccess = targetSuccessChance.Evaluate(variables);

            float targetRoll = rand.Next(0, 100);
            if(targetSuccess >= targetRoll)
            {
                foreach (string attribute in targetDiff.Keys)
                {
                    Target.attributes[attribute] += targetDiff[attribute].Evaluate(variables);
                }

            }
            float sourceRoll = rand.Next(0, 100);
            if(sourceSuccess >= sourceRoll)
            {
                foreach (string attribute in sourceDiff.Keys)
                {
                    Source.attributes[attribute] += sourceDiff[attribute].Evaluate(variables);
                }
            }
        }

    }
}
