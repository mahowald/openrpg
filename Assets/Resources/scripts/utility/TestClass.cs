using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class TestClass : MonoBehaviour {
    
	void Start () {
        TestEquationParser();
	}
	
    void Update()
    {
    }
    void TestEquationParser()
    {
        ExpressionParser ep = new ExpressionParser();
        
        List<string> equation = new List<string> { "(", "1", "+", "a", ")", "*", "3", "-", "4", "/", "5" }; // should be 8.2
        string splitstr = "[";
        List<string> toParse = ep.SplitExpression("(1 + alph.x)*3 - 4/5");
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
        Queue<string> eqQueue = ep.ConvertToPostfix(toParse);
        float value = ep.EvaluateExpression(eqQueue, variables);

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
    
}
