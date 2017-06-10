using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

// A class to parse a string equation
// and evaluate the result. 
public class Expression {

    // When we get a new expression, we store it in postfix notation
    private Queue<string> pfEquation;

    public Expression(string equation)
    {
        pfEquation = Expression.ConvertToPostfix(Expression.SplitExpression(equation));
    }

    public Expression()
    {
        pfEquation = null;
    }

    public string Equation
    {
        get
        {
            return Expression.ConvertToInfix(pfEquation);
        }
        set
        {
            pfEquation = Expression.ConvertToPostfix(Expression.SplitExpression(value));
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

    static Dictionary<string, int> functionsArity = new Dictionary<string, int>()
    {
        {"if",3 }, // if(x, t, f) returns t if x > 0, f otherwise
        {"max", 2 }, // max(a,b) returns the larger of a and b
        {"min", 2 }, // min(a,b) returns the smaller of a and b
        {"abs", 1 }, // abs(a) returns absolute value of a
        {"sin", 1 }, // sin(a) returns the sine of a, where a in radians
        {"cos", 1 }, // cos(a) returns the cosine of a, where a in radians
        {"pi", 0 }, // pi() returns the value of pi, i.e., 3.1415...
        {"floor", 1 }, // floor(x) returns the largest integer <= x.
        {"ceil", 1 }, // ceil(x) returns the smallest integer >= x.
        {"log", 1 }, // log(x) returns the natural log of x
        {"exp", 1 }, // exp(x) : e^x
        {"logit", 1 } // logit(x) : returns 1/(1 + exp(-x))
    };
    
    public static List<string> SplitExpression(string input)
    {
        List<string> operators = new List<string>(operatorPrecedence.Keys);
        string[] rawParts = Regex.Split(input, "(?=[,)(*+/-])|(?<=[,)(*+/-])");

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

    // Convert a postfix expression to an infix one.
    public static string ConvertToInfix(Queue<string> postfix)
    {
        Queue<string> expression = new Queue<string>(postfix);
        Stack<string> output = new Stack<string>();
        List<string> operators = new List<string>(operatorPrecedence.Keys);

        while (expression.Count > 0)
        {
            string element = expression.Dequeue();
            if (operators.Contains(element))
            {
                string a = output.Pop();
                string b = output.Pop();
                string result = "(" + b + element + a + ")";
                output.Push(result);
            }
            else
            {
                output.Push(element);
            }

        }
        return output.Pop();
    }

    // Converts an infix (i.e. normal) expression to a postfix one
    public static Queue<string> ConvertToPostfix(List<string> input)
    {
        Queue<string> output = new Queue<string>();
        Stack<string> operatorStack = new Stack<string>();
        List<string> operators = new List<string>(operatorPrecedence.Keys);
        List<string> functions = new List<string>(functionsArity.Keys);

        for (int i = 0; i < input.Count; i++)
        {
            string element = input[i];
            if(operators.Contains(element))
            {
                int precedence = operatorPrecedence[element];
                if(operatorStack.Count == 0 || operatorStack.Peek() == "(" || functions.Contains(operatorStack.Peek()))
                    operatorStack.Push(element);
                else
                {
                    string peek = operatorStack.Peek();
                    while (operatorStack.Count > 0 && peek != "(" && !functions.Contains(peek))
                    {
                        peek = operatorStack.Peek();
                        int priorPrecedence = operatorPrecedence[peek];
                        if (precedence >= priorPrecedence)
                            output.Enqueue(operatorStack.Pop());
                        else
                            break;
                    }
                    operatorStack.Push(element);
                }
            }
            else if (functions.Contains(element))
            {
                operatorStack.Push(element);
            }
            else if (element == ",")
            {
                while (operatorStack.Count > 0 && operatorStack.Peek() != "(")
                    output.Enqueue(operatorStack.Pop());
            }
            else if (element == ")")
            {
                while(operatorStack.Count > 0 && operatorStack.Peek() != "(")
                    output.Enqueue(operatorStack.Pop());
                if(operatorStack.Peek() == "(")
                    operatorStack.Pop();
                if (operatorStack.Count > 0 && functions.Contains(operatorStack.Peek()))
                    output.Enqueue(operatorStack.Pop());
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
        List<string> functions = new List<string>(functionsArity.Keys);
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
            else if (functions.Contains(element))
            {
                List<float> arguments = new List<float>(functionsArity[element]);
                for(int i = 0; i < functionsArity[element]; i++)
                {
                    arguments.Add(values.Pop());
                }
                arguments.Reverse();
                float result = float.NaN;
                switch (element)
                {
                    case "if":
                        if (arguments[0] > 0)
                            result = arguments[1];
                        else
                            result = arguments[2];
                        break;
                    case "max":
                        if (arguments[0] > arguments[1])
                            result = arguments[0];
                        else
                            result = arguments[1];
                        break;
                    case "min":
                        if (arguments[0] < arguments[1])
                            result = arguments[0];
                        else
                            result = arguments[1];
                        break;
                    case "abs":
                        result = Mathf.Abs(arguments[0]);
                        break;
                    case "sin":
                        result = Mathf.Sin(arguments[0]);
                        break;
                    case "cos":
                        result = Mathf.Cos(arguments[0]);
                        break;
                    case "floor":
                        result = Mathf.Floor(arguments[0]);
                        break;
                    case "ceil":
                        result = Mathf.Ceil(arguments[0]);
                        break;
                    case "log":
                        result = Mathf.Log(arguments[0]);
                        break;
                    case "exp":
                        result = Mathf.Exp(arguments[0]);
                        break;
                    case "logit":
                        result = 1f / (1 + Mathf.Exp(-1f * arguments[0]));
                        break;
                    case "pi":
                        result = Mathf.PI;
                        break;
                }
                if (result != float.NaN)
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
