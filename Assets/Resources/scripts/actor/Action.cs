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
    public interface IAction<T>
    {
        T Target { get; }
        Actor Source { get; }
        void DoAction();
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
