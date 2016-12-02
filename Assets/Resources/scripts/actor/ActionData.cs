using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ActorSystem
{
    public struct ActionData
    {
        Dictionary<string, float> hostileAttributeMod; // hostile effects are mitigated by armor; resistances
        Dictionary<string, float> friendlyAttributeMod; // friendly effects are not mitigated by armor or resistances
    }
}
