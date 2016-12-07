using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ActorSystem
{
    public struct ActionData
    {
        public Dictionary<string, float> attributeModifier;
        public Geometry.Arc arc;
        public bool bypassResistance;
        public Dictionary<Effect, float> effects; // effects and their durations
        public float range; // how far away before it can be used
        public bool ranged;
    }
}
