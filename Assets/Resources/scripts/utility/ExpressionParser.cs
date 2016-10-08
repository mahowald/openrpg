using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

// A class to parse a string equation
// and evaluate the result. 
public class ExpressionParser {
    
    // record the order of operations
    Dictionary<string, int> operatorPrecedence = new Dictionary<string, int>()
    { 
        {"(", 0 }, {"^", 1 }, {"*", 2 }, {"/", 3 }, {"+", 4 }, {"-", 5 }
    };
    
    public List<string> SplitExpression(string input)
    {
        string[] parts = Regex.Split(input, "(?=[)(*+/-])|(?<=[)(*+/-])");
        List<string> output = new List<string>();
        foreach(string part in parts)
        {
            if(part == "")
            {
                continue;
            }
            output.Add(part.Trim());
        }
        return output;
    }

    // Converts an infix (i.e. normal) expression to a postfix one
    public Queue<string> ConvertToPostfix(List<string> input)
    {
        Queue<string> output = new Queue<string>();
        Stack<string> operatorStack = new Stack<string>();
        List<string> operators = new List<string>(operatorPrecedence.Keys);

        for (int i = 0; i < input.Count; i++)
        {
            string element = input[i];
            if(operators.Contains(element))
            {
                int precedence = operatorPrecedence[element];
                if(operatorStack.Count == 0 || operatorStack.Peek() == "(")
                    operatorStack.Push(element);
                else
                {
                    while(operatorStack.Count > 0 && operatorStack.Peek() != "(")
                    {
                        int priorPrecedence = operatorPrecedence[operatorStack.Peek()];
                        if (precedence >= priorPrecedence )
                            output.Enqueue(operatorStack.Pop());
                        else
                            break;
                    }
                    operatorStack.Push(element);
                }
            }
            else if (element == ")")
            {
                while(operatorStack.Count > 0 && operatorStack.Peek() != "(")
                    output.Enqueue(operatorStack.Pop());
                if(operatorStack.Peek() == "(")
                    operatorStack.Pop();
            }
            else // a number or variable
                output.Enqueue(element);
        }
        while (operatorStack.Count > 0)
            output.Enqueue(operatorStack.Pop());
        return output;
    }

    // evaluates a postfix expression
    public float EvaluateExpression(Queue<string> input, Dictionary<string, float> variables)
    {
        Queue<string> expression = new Queue<string>(input);
        Debug.Log(expression.Count);
        Stack<float> values = new Stack<float>();
        List<string> operators = new List<string>(operatorPrecedence.Keys);
        while (expression.Count > 0)
        {
            string element = expression.Dequeue();
            if(operators.Contains(element))
            {
                float a = values.Pop();
                float b = values.Pop();
                float result = float.NaN;
                switch(element)
                {
                    case "+":
                        result = b + a;
                        break;
                    case "-":
                        result = b - a;
                        break;
                    case "*":
                        result = b * a;
                        break;
                    case "/":
                        result = b / a;
                        break;
                    case "^":
                        result = (float) System.Math.Pow(b, a);
                        break;
                }
                if(result != float.NaN)
                    values.Push(result);
            }
            else
            {
                if (variables.ContainsKey(element))
                    values.Push(variables[element]);
                else
                    values.Push(float.Parse(element, System.Globalization.CultureInfo.InvariantCulture));
            }
        }
        return values.Pop();
    }

    public float EvaluateExpression(Queue<string> input)
    {
        return EvaluateExpression(input, new Dictionary<string, float>());
    }


}
