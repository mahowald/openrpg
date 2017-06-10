using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

// A class to parse a string equation
// and evaluate the result. 
public class Expression {

    // When we get a new expression, we store it in postfix notation
    private Queue<string> pfEquation;

    // the equation as a string
    private string equation;

    public Expression(string equation)
    {
        pfEquation = Expression.ConvertToPostfix(Expression.SplitExpression(equation));
        this.equation = equation;
    }

    public Expression()
    {
        pfEquation = null;
        equation = null;
    }

    public string Equation
    {
        get
        {
            return equation;
        }
        set
        {
            pfEquation = Expression.ConvertToPostfix(Expression.SplitExpression(value));
            this.equation = value;
        }
    }

    public float Evaluate(Dictionary<string, float> variables)
    {
        return Expression.EvaluateExpression(pfEquation, variables);
    }

    public float Evaluate()
    {
        return Evaluate(new Dictionary<string, float>());
    }

    // record the order of operations
    static Dictionary<string, int> operatorPrecedence = new Dictionary<string, int>()
    { 
        {"(", 0 }, {"^", 1 }, {"*", 2 }, {"/", 3 }, {"+", 4 }, {"-", 5 }
    };
    
    public static List<string> SplitExpression(string input)
    {
        List<string> operators = new List<string>(operatorPrecedence.Keys);
        string[] rawParts = Regex.Split(input, "(?=[)(*+/-])|(?<=[)(*+/-])");

        List<string> parts = new List<string>(rawParts);
        parts.RemoveAll( s => s.Trim() == "");

        List<string> output = new List<string>();
        var i = 0;

        while(i < parts.Count)
        { 
            string part = parts[i];
            part = part.Trim();
            if (part == "-" && (i == 0 || operators.Contains(parts[i - 1])) ) // Subtraction versus a negative number
            {
                output.Add("-1");
                output.Add("*");
                i++;
            }
            else
            {
                output.Add(part);
                i++;
            }
        }
        return output;
    }

    // Converts an infix (i.e. normal) expression to a postfix one
    public static Queue<string> ConvertToPostfix(List<string> input)
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
    public static float EvaluateExpression(Queue<string> input, Dictionary<string, float> variables)
    {
        Queue<string> expression = new Queue<string>(input);
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

    public static float EvaluateExpression(Queue<string> input)
    {
        return EvaluateExpression(input, new Dictionary<string, float>());
    }

    // Serialization trick
    public static implicit operator Expression(string value)
    {
        return new Expression(value);
    }

}
