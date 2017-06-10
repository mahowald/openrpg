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
        TestSerializeActionMapper();
	}
	
    void Update()
    {
    }

    void TestSerializeActionMapper()
    {

        // attack prototype
        var attackPrototype = new ActorSystem.SingleTargetDamageActionPrototype();
        attackPrototype.Cooldown = new Expression("0");
        attackPrototype.Cost = new Dictionary<string, Expression>();
        attackPrototype.Damage = new Dictionary<string, Expression>(); //  { { "health", new Expression("5") } };
        attackPrototype.Range = new Expression("1");
        attackPrototype.Animation = ActorSystem.ActionAnimation.BasicAttack;

        // move prototype
        var movePrototype = new ActorSystem.LocatableEmptyActionPrototype();

        var serializer = new SerializerBuilder().Build();
        var yaml = serializer.Serialize(attackPrototype);

        Debug.Log(yaml);

        var actionMapperStr = @"---
actionBag:
  Location:
  - {}
  Actor:
  - damage: {}
    animation: BasicAttack
    cooldown:
      Equation: 0
    effects: {}
    cost: {}
    range:
      Equation: 1
    success_chance:
      Equation: 0
    critical_chance:
      Equation: 0
    critical_effects: {}
";
        var prototypestr = @"
damage:
    health: -5
animation: BasicAttack
cooldown: 0
effects: {}
cost: {}
range: 1
success_chance: 0
critical_chance: 0
critical_effects: {}
";

        /** this is equivalent to
        damage:
        health:
            Equation: -5
        animation: BasicAttack
        cooldown: {}
        effects: {}
        cost: {}
        range: {}
        success_chance: {}
        critical_chance: {}
        critical_effects: {}


        **/


        var deserializer = new DeserializerBuilder().Build();
        var prototyped = deserializer.Deserialize<ActorSystem.SingleTargetDamageActionPrototype>(prototypestr);

        Debug.Log(serializer.Serialize(prototyped));
        
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

        Debug.Log(Expression.ConvertToInfix(eqQueue));

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

        List<string> toParse2 = Expression.SplitExpression("1 + if( (2+3), 4, 5) - 6"); // = -1
        Queue<string> eqQueue2 = Expression.ConvertToPostfix(toParse2);
        float value2 = Expression.EvaluateExpression(eqQueue2);
        string out2 = "";
        foreach(string x in toParse2)
        {
            out2 += x + " ";
        }
        out2 += "--> ";
        while (eqQueue2.Count > 0)
        {
            out2 += eqQueue2.Dequeue() + " ";
        }
        out2 += " = " + value2.ToString();
        Debug.Log(out2);

        Debug.Log(new Expression("max(2,3)").Evaluate()); // 3
        Debug.Log(new Expression("1 + pi()").Evaluate()); // 4.14159
        Debug.Log(new Expression("logit(0)").Evaluate()); // 0.5
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
