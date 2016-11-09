using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using ActorSystem;

/// <summary>
/// Utility class to compute geometric things, like which actors are within which region
/// </summary>
public static class Geometry {
    
    public struct Arc
    {
        public float radius;
        public float angle;
        public Vector2 origin;
        public Vector2 direction;
        
        public Arc(float radius, float angle, Vector2 origin, Vector2 direction)
        {
            this.radius = radius;
            this.angle = angle;
            this.origin = origin;
            this.direction = direction;
        }

        public Arc(float radius, float angle, Vector3 origin, Vector3 direction)
        {
            this.radius = radius;
            this.angle = angle;
            this.origin = new Vector2(origin.x, origin.z);
            this.direction = (new Vector2(direction.x, direction.z)).normalized;
        }
    }

    /// <summary>
    /// Returns a list of the actors contained in the specified arc, satsifying the filter.
    /// </summary>
    /// <param name="arc">The arc to check</param>
    /// <param name="filter">Only check Actors which satisfy this condition</param>
    /// <returns></returns>
    public static List<Actor> GetActorsInArc(Arc arc, Func<Actor, bool> filter)
    {
        List<Actor> actors = new List<Actor>();
        Vector2 origin = arc.origin;
        float scale = arc.radius;
        float angle = Mathf.PI * arc.angle / 180f; // convert to radians
        float cos = Mathf.Cos(angle/2f);
        float sin = Mathf.Sin(angle/2f);

        Vector2 left = new Vector2(cos * arc.direction.x - sin * arc.direction.y, 
                                   sin * arc.direction.x + cos * arc.direction.y);
        Vector2 right = new Vector2(cos * arc.direction.x + sin * arc.direction.y, 
                                   -sin * arc.direction.x + cos * arc.direction.y);

        Vector2 leftPerp = new Vector2(left.y, -left.x);
        Vector2 rightPerp = new Vector2(-right.y, right.x);

        foreach(Actor a in GameController.Actors)
        {

            if (!filter(a))
                continue;

            Vector2 pos = (new Vector2(a.Position.x, a.Position.z) - origin)/scale; // set the origin to (0, 0) and normalize

            if (pos.magnitude - a.Radius/scale > 1) // too far away to consider. 
                continue;

            Vector2 leftPos = pos + (a.Radius / scale) * leftPerp; // closest point to the left boundary
            Vector2 rightPos = pos + (a.Radius / scale) * rightPerp; // closest point to the right boundary
            

            float centerAngle = Vector2.Angle(pos, arc.direction);
            float leftAngle = Vector2.Angle(leftPos, arc.direction);
            float rightAngle = Vector2.Angle(rightPos, arc.direction);


            if (Mathf.Abs(centerAngle) < arc.angle / 2f)
                actors.Add(a);
            else if (Mathf.Abs(leftAngle) < arc.angle / 2f)
                actors.Add(a);
            else if (Math.Abs(rightAngle) < arc.angle / 2f)
                actors.Add(a);
        }

        return actors;
    }


}
