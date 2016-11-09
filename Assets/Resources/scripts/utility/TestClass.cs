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
        Deserializer deserializer = new Deserializer();
        Expression deexp = deserializer.Deserialize<Expression>(new StringReader("source.x + 5"));
        // Debug.Log(deexp.Evaluate(new Dictionary<string, float> { { "source.x", 3f } })); // should be 8

        Expression myExp = new Expression("source.x + 5");
        Dictionary<string, Expression> myExps = new Dictionary<string, Expression> { { "health", myExp } };
        Dictionary<string, float> myVariables = new Dictionary<string, float> { { "source.health", 10f } };
        ActorSystem.ActorTransactionPrototype aproto = new ActorSystem.ActorTransactionPrototype(myExps, new Dictionary<string, Expression>());

        //Serializer serializer = new Serializer();
        //StringBuilder stringBuilder = new StringBuilder();
        //StringWriter stringWriter = new StringWriter(stringBuilder);
        //serializer.Serialize(stringWriter, myExp);

        ActorSystem.ProbabalisticActorTransactionPrototype aproto2 = deserializer.Deserialize<ActorSystem.ProbabalisticActorTransactionPrototype>(new StringReader(Document));
        foreach(Expression exp in aproto2.TargetDiff.Values)
        {
            Debug.Log(exp.Evaluate(myVariables)); // should produce 8
        }
        // Dictionary<string, Dictionary<string, string>> myDict = deserializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(new StringReader(Document));
        // Debug.Log(myDict["target_diff"]["health"]);
        // Debug.Log(stringBuilder);

        actor1.attributes = new ActorSystem.Attributes(new Dictionary<string, float>());
        actor1.attributes.attributes.Add("health", 10f);
        Debug.Log(actor1.attributes["health"]); // should be 10
        ActorSystem.IAction<ActorSystem.Actor> myAction = aproto2.Instantiate(actor1, actor1);
        myAction.DoAction();
        Debug.Log(actor1.attributes["health"]); // should be 13 or 10
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
