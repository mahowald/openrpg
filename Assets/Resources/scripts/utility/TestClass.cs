using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using System.Text; // string builder
using System.IO;

public class TestClass : MonoBehaviour {

    public ActorSystem.Actor actor1;

	void Start () {
        // TestEquationParser();
        // TestActionPrototype();
	}
	
    void Update()
    {
    }
    void TestEquationParser()
    {
        
        List<string> equation = new List<string> { "(", "1", "+", "a", ")", "*", "3", "-", "4", "/", "5" }; // should be 8.2
        string splitstr = "[";
        List<string> toParse = Expression.SplitExpression("(1 + alph.x)*3 - 4/5");
        for (int i = 0; i < toParse.Count; i++)
        {
            splitstr += toParse[i];
            if (i != toParse.Count - 1)
                splitstr += ",";
        }
        splitstr += "]";

        Dictionary<string, float> variables = new Dictionary<string, float>
        {
            {"alph.x",2f }
        };

        string output = "";
        Queue<string> eqQueue = Expression.ConvertToPostfix(toParse);
        float value = Expression.EvaluateExpression(eqQueue, variables);

        foreach (string elem in toParse)
        {
            output += elem;
        }
        output += " --> ";
        while (eqQueue.Count > 0)
        {
            output += eqQueue.Dequeue();
        }
        Debug.Log(output + " = " + value.ToString());
    }
    
    void TestActionPrototype()
    {
        return;
    }

    public void TestActions(Geometry.Arc arc)
    {
        List<ActorSystem.Actor> actors = Geometry.GetActorsInArc(arc, a => a != SelectorBase.SourceActor);
        foreach(ActorSystem.Actor a in actors)
        {
            a.HandleAction(new ActorSystem.ActionData());
        }
    }

    public void OnEnable()
    {
        EventManager.StartListening<Geometry.Arc>("Arc Selected", TestActions);
    }

    public void OnDisable()
    {
        EventManager.StopListening<Geometry.Arc>("Arc Selected", TestActions);
    }

    /**
    public void ButtonPressed()
    {
        Debug.Log("Button Pressed.");
    }

    // Subscribe/Unsubscribe to our events
    public void OnEnable()
    {
        EventManager.StartListening("ButtonPressed", ButtonPressed);
    }
    public void OnDisable()
    {
        EventManager.StopListening("ButtonPressed", ButtonPressed);
    }
    **/

    private const string Document = @"---
        target_diff:
            health: 2*source.health/5 - 1
        source_diff:
            health: 0
        target_chance: 50
        
...";
    
}
