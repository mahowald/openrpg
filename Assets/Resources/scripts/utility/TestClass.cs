using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TestClass : MonoBehaviour {

	// Use this for initialization
	void Start () {
        TestEquationParser();
	}
	
    void TestEquationParser()
    {
        ExpressionParser ep = new ExpressionParser();
        List<string> equation = new List<string> { "(", "1", "+", "2", ")", "*", "3", "-", "4", "/", "5" }; // should be 8.2

        string output = "";
        Queue<string> eqQueue = ep.ConvertToPostfix(equation);
        float value = ep.EvaluateExpression(eqQueue);

        foreach (string elem in equation)
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

	// Update is called once per frame
	void Update () {
	
	}
}
