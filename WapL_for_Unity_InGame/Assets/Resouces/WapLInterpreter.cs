using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;

class Variable
{
    public string Type; // "number", "text", "bool", "vec3", "gameobject", "component"
    public string Value;
}

class Function
{
    public List<(string Type, string Name)> Parameters = new List<(string, string)>();
    public List<string> Body = new List<string>();
}

public class WapLInterpreter : MonoBehaviour
{
    static Dictionary<string, Variable> variables = new Dictionary<string, Variable>();
    static Dictionary<string, Function> functions = new Dictionary<string, Function>();
    static Dictionary<string, int> labelPositions = new Dictionary<string, int>();
    public InputField inputfield;
    public string input;
    public Text outputfield;
    public string output;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ReadInput()
    {
        input = inputfield.text;
    }
    public void ReadInputFromString(string code)
    {
        input = code;
    }

    public float RunCode()
    {
        float used_energy = 0.0f;
        string[] commands = input.Split(';');

        // ���x���ʒu�̃X�L����
        for (int i = 0; i < commands.Length; i++)
        {
            string line = commands[i].Trim();
            if (line.StartsWith("point "))
            {
                string labelName = line.Substring(6).Trim();
                labelPositions[labelName] = i;
            }
        }
        //�֐��̃X�L����
        for (int i = 0; i < commands.Length; i++)
        {
            string trimmed = commands[i].Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            if (trimmed.StartsWith("func "))
            {
                string head = trimmed.Substring(5);
                int lparen = head.IndexOf('(');
                int rparen = head.IndexOf(')');
                string funcName = head.Substring(0, lparen).Trim();
                string argsPart = head.Substring(lparen + 1, rparen - lparen - 1);

                var parameters = new List<(string, string)>();
                foreach (var p in argsPart.Split(','))
                {
                    var parts = p.Trim().Split(' ');
                    if (parts.Length == 2)
                    {
                        parameters.Add((parts[0], parts[1]));
                    }
                }

                List<string> body = new List<string>();
                i++;
                while (i < commands.Length && !commands[i].Trim().StartsWith("}"))
                {
                    body.Add(commands[i].Trim());
                    i++;
                }
                functions[funcName] = new Function
                {
                    Parameters = parameters,
                    Body = body
                };
                continue;
            }
        }

        for (int i = 0; i < commands.Length; i++)
        {
            string trimmed = commands[i].Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            if (trimmed.StartsWith("fn "))
            {
                string head = trimmed.Substring(5);
                int lparen = head.IndexOf('(');
                int rparen = head.IndexOf(')');
                string funcName = head.Substring(0, lparen).Trim();
                string argsPart = head.Substring(lparen + 1, rparen - lparen - 1);

                var parameters = new List<(string, string)>();
                foreach (var p in argsPart.Split(','))
                {
                    var parts = p.Trim().Split(' ');
                    if (parts.Length == 2)
                    {
                        parameters.Add((parts[0], parts[1]));
                    }
                }

                List<string> body = new List<string>();
                i++;
                while (i < commands.Length && !commands[i].Trim().StartsWith("}"))
                {
                    body.Add(commands[i].Trim());
                    i++;
                }
                continue;
            }

            if (trimmed.StartsWith("warpto("))
            {
                string labelName = trimmed.Substring(7, trimmed.Length - 8).Trim();
                if (labelPositions.ContainsKey(labelName))
                {
                    i = labelPositions[labelName];
                    continue;
                }
                else
                {
                    //Console.WriteLine("���x����������܂���: " + labelName);
                    output += "\n���x����������܂���: " + labelName;
                    outputfield.text = output;
                }
            }

            // warptoif(����, ���x����) �̏���
            if (trimmed.StartsWith("warptoif("))
            {
                string inner = trimmed.Substring(9, trimmed.Length - 10);
                string[] parts = SplitArgs(inner);
                if (parts.Length == 2)
                {
                    string conditionResult = EvaluateExpression(parts[0].Trim());
                    string label = parts[1].Trim();
                    if (conditionResult == "true" && labelPositions.ContainsKey(label))
                    {
                        i = labelPositions[label];
                        continue;
                    }
                }
            }

            EvaluateCommand(trimmed);
        }



        return used_energy;

    }

    string EvaluateCommand(string line, Dictionary<string, Variable>? localScope = null)
    {
        if (line.StartsWith("Print "))
        {
            string val = EvaluateExpression(line.Substring(6), localScope);
            output += "\n" + val;
            outputfield.text = output;
        }
        else if (line.StartsWith("let number "))
        {
            string[] parts = line.Substring(11).Split('=');
            string name = parts[0].Trim();
            string value = EvaluateExpression(parts[1].Trim(), localScope);
            SetVariable(name, "number", value, localScope);
        }
        else if (line.StartsWith("let text "))
        {
            string[] parts = line.Substring(9).Split('=');
            string name = parts[0].Trim();
            string value = parts[1].Trim().Trim('"');
            SetVariable(name, "text", value, localScope);
        }
        else if (line.StartsWith("let bool "))
        {
            string[] parts = line.Substring(9).Split('=');
            string name = parts[0].Trim();
            string value = EvaluateExpression(parts[1].Trim(), localScope);
            SetVariable(name, "bool", value, localScope);
        }
        else if (line.StartsWith("Input "))
        {
            //�Q�[�����ŃR���\�[���̓��͎͂󂯂Ȃ��̂Ŕp�~
            //string name = line.Substring(6).Trim();
            //Console.Write($"���� [{name}]: ");
            //string value = Console.ReadLine() ?? "";
            //SetVariable(name, "text", value, localScope);
        }
        else if (line.Contains("(") && line.EndsWith(")"))
        {
            string funcName = line.Substring(0, line.IndexOf('(')).Trim();
            string argsPart = line.Substring(line.IndexOf('(') + 1);
            argsPart = argsPart.Substring(0, argsPart.Length - 1);
            string[] arguments = SplitArgs(argsPart);

            if (functions.ContainsKey(funcName))
            {
                var func = functions[funcName];
                var localVars = new Dictionary<string, Variable>();
                for (int j = 0; j < func.Parameters.Count; j++)
                {
                    string val = EvaluateExpression(arguments[j].Trim(), localScope);
                    string type = func.Parameters[j].Type;
                    localVars[func.Parameters[j].Name] = new Variable { Type = type, Value = val };
                }

                string result = ExecuteFunctionBody(func.Body, localVars);
                return result;
            }
            else
            {
                EvaluateExpression(line, localScope);
            }
        }
        return "";
    }
    string ExecuteFunctionBody(List<string> body, Dictionary<string, Variable> scope)
    {
        // �֐��������̃��x���\
        var localLabels = new Dictionary<string, int>();
        for (int i = 0; i < body.Count; i++)
        {
            string line = body[i].Trim();
            if (line.StartsWith("point "))
            {
                string labelName = line.Substring(6).Trim();
                localLabels[labelName] = i;
            }
        }

        for (int i = 0; i < body.Count; i++)
        {
            string line = body[i].Trim();
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (line.StartsWith("warpto("))
            {
                string labelName = line.Substring(7, line.Length - 8).Trim();
                if (localLabels.ContainsKey(labelName))
                {
                    i = localLabels[labelName];
                    continue;
                }
                else
                {
                    //Console.WriteLine("�֐����̃��x����������܂���: " + labelName);
                    output += "�֐����̃��x����������܂���: " + labelName;
                    outputfield.text = output;
                }
            }

            if (line.StartsWith("warptoif("))
            {
                string inner = line.Substring(9, line.Length - 10);
                string[] parts = SplitArgs(inner);
                if (parts.Length == 2)
                {
                    string condition = EvaluateExpression(parts[0].Trim(), scope);
                    string label = parts[1].Trim();
                    if (condition == "true" && localLabels.ContainsKey(label))
                    {
                        i = localLabels[label];
                        continue;
                    }
                }
            }


            if (line.StartsWith("return "))
            {
                string retExpr = line.Substring(7).Trim();
                return EvaluateExpression(retExpr, scope); // �� �����Œl��Ԃ�
            }

            EvaluateCommand(line, scope);
        }
        return "";
    }
    string EvaluateExpression(string exprInput, Dictionary<string, Variable>? scope = null)
    {
        exprInput = exprInput.Trim();
        if (exprInput.StartsWith("\"") && exprInput.EndsWith("\"")) return exprInput.Substring(1, exprInput.Length - 2);
        if (double.TryParse(exprInput, out double n)) return n.ToString();
        if ((scope != null && scope.ContainsKey(exprInput))) return scope[exprInput].Value;
        if (variables.ContainsKey(exprInput)) return variables[exprInput].Value;

        if (exprInput.Contains("(") && exprInput.EndsWith(")"))
        {
            int lparen = exprInput.IndexOf('(');
            string op = exprInput.Substring(0, lparen);
            string inside = exprInput.Substring(lparen + 1, exprInput.Length - lparen - 2);
            string[] parts = SplitArgs(inside);

            List<string> evalpart = new List<string>(parts.Length);
            if (op != "do")
            {
                for (int l = 0; l < parts.Length; l++)
                {
                    evalpart.Add(parts.Length > l ? EvaluateExpression(parts[l], scope) : "");
                }
            }

            if(variables.ContainsKey(op) && variables[op].Type == "vec3")
            {
                string exprInputV3 = variables[op].Value;
                int lparenV3 = exprInputV3.IndexOf('(');
                string opV3 = exprInputV3.Substring(0, lparenV3);
                string insideV3 = exprInput.Substring(lparenV3 + 1, exprInputV3.Length - lparenV3 - 2);
                string[] partsV3 = SplitArgs(insideV3);
                if (parts[0] == "x")
                {
                    return partsV3[0];
                }
                else if(parts[0] == "y")
                {
                    return partsV3[1];
                }
                else if (parts[0] == "z")
                {
                    return partsV3[2];
                }
            }

            switch (op)
            {
                case "+": return (double.Parse(evalpart[0]) + double.Parse(evalpart[1])).ToString();
                case "t+": return (evalpart[0] + evalpart[1]).ToString();
                case "-": return (double.Parse(evalpart[0]) - double.Parse(evalpart[1])).ToString();
                case "*": return (double.Parse(evalpart[0]) * double.Parse(evalpart[1])).ToString();
                case "t*": string textadd = ""; for (int i = 1; i <= int.Parse(evalpart[1]); i++) { textadd += evalpart[0]; } return textadd;
                case "/": return (double.Parse(evalpart[1]) != 0 ? double.Parse(evalpart[0]) / double.Parse(evalpart[1]) : 0).ToString();
                case "%": return (double.Parse(evalpart[1]) != 0 ? double.Parse(evalpart[0]) % double.Parse(evalpart[1]) : 0).ToString();
                case "==": return (evalpart[0] == evalpart[1]).ToString().ToLower();
                case "!=": return (evalpart[0] != evalpart[1]).ToString().ToLower();
                case ">": return (double.Parse(evalpart[0]) > double.Parse(evalpart[1])).ToString().ToLower();
                case "<": return (double.Parse(evalpart[0]) < double.Parse(evalpart[1])).ToString().ToLower();
                case ">=": return (double.Parse(evalpart[0]) >= double.Parse(evalpart[1])).ToString().ToLower();
                case "<=": return (double.Parse(evalpart[0]) <= double.Parse(evalpart[1])).ToString().ToLower();
                case "and": return ((evalpart[0] == "true") && (evalpart[1] == "true")).ToString().ToLower();
                case "or": return ((evalpart[0] == "true") || (evalpart[1] == "true")).ToString().ToLower();
                case "not": return (evalpart[0] != "true").ToString().ToLower();
                case "+=": if ((scope != null && scope.ContainsKey(parts[0]))) { SetVariable(parts[0], scope[parts[0]].Type, (double.Parse(evalpart[0]) + double.Parse(evalpart[1])).ToString(), scope); } else if (variables.ContainsKey(parts[0])) { SetVariable(parts[0], variables[parts[0]].Type, (double.Parse(evalpart[0]) + double.Parse(evalpart[1])).ToString(), scope); } return (double.Parse(evalpart[0]) + double.Parse(evalpart[1])).ToString();
                case "-=": if ((scope != null && scope.ContainsKey(parts[0]))) { SetVariable(parts[0], scope[parts[0]].Type, (double.Parse(evalpart[0]) - double.Parse(evalpart[1])).ToString(), scope); } else if (variables.ContainsKey(parts[0])) { SetVariable(parts[0], variables[parts[0]].Type, (double.Parse(evalpart[0]) - double.Parse(evalpart[1])).ToString(), scope); } return (double.Parse(evalpart[0]) - double.Parse(evalpart[1])).ToString();
                case "*=": if ((scope != null && scope.ContainsKey(parts[0]))) { SetVariable(parts[0], scope[parts[0]].Type, (double.Parse(evalpart[0]) * double.Parse(evalpart[1])).ToString(), scope); } else if (variables.ContainsKey(parts[0])) { SetVariable(parts[0], variables[parts[0]].Type, (double.Parse(evalpart[0]) * double.Parse(evalpart[1])).ToString(), scope); } return (double.Parse(evalpart[0]) * double.Parse(evalpart[1])).ToString();
                case "/=": if (double.Parse(evalpart[1]) != 0) { if ((scope != null && scope.ContainsKey(parts[0]))) { SetVariable(parts[0], scope[parts[0]].Type, (double.Parse(evalpart[0]) / double.Parse(evalpart[1])).ToString(), scope); } else if (variables.ContainsKey(parts[0])) { SetVariable(parts[0], variables[parts[0]].Type, (double.Parse(evalpart[0]) / double.Parse(evalpart[1])).ToString(), scope); } return (double.Parse(evalpart[0]) / double.Parse(evalpart[1])).ToString(); } return "";
                case "%=": if (double.Parse(evalpart[1]) != 0) { if ((scope != null && scope.ContainsKey(parts[0]))) { SetVariable(parts[0], scope[parts[0]].Type, (double.Parse(evalpart[0]) % double.Parse(evalpart[1])).ToString(), scope); } else if (variables.ContainsKey(parts[0])) { SetVariable(parts[0], variables[parts[0]].Type, (double.Parse(evalpart[0]) % double.Parse(evalpart[1])).ToString(), scope); } return (double.Parse(evalpart[0]) % double.Parse(evalpart[1])).ToString(); } return "";
                case "=":
                    if (parts.Length < 3) return "";
                    string type = evalpart[0].Trim();  // parts[0]
                    string name = evalpart[1].Trim();  // parts[1]
                    string value = evalpart[2].Trim(); // parts[2]
                    if (type == "number" || type == "bool"||type == "vec3")
                    {
                        string eval = EvaluateExpression(value, scope);
                        SetVariable(parts[1], type, eval, scope);
                        return eval;
                    }
                    else if (type == "text")
                    {
                        // ������Ȃ�݂͂�����
                        if (value.StartsWith("\"") && value.EndsWith("\""))
                        {
                            value = value.Substring(1, value.Length - 2);
                        }
                        SetVariable(name, type, value, scope);
                        return value;
                    }
                    return "";
                case "input":
                    //�Q�[�����ŃR���\�[���̓��͎͂󂯂Ȃ��̂Ŕp�~
                    //string input_name = evalpart[0];
                    //Console.Write($"���� [{input_name}]: ");
                    //string input_value = Console.ReadLine() ?? "";
                    //return input_value;
                    return "";
                case "print":
                    for (int i = 0; i <= parts.Length - 1; i++)
                    {
                        //Console.WriteLine(evalpart[i]);
                        output += "\n" + evalpart[i];
                        outputfield.text = output;
                    }

                    return evalpart[0];
                case "if":
                    if (evalpart[0] == "true") { return evalpart[1]; } else { return evalpart[2]; }
                case "do":
                    var localVars = new Dictionary<string, Variable>();
                    List<string> todo = new List<string>();
                    for (int i = 0; i <= parts.Length - 1; i++)
                    {
                        todo.Add(parts[i].Trim());
                    }

                    string result = ExecuteFunctionBody(todo, localVars);
                    return result;
                case "vec3":
                    string vector_three = "vec3(" + evalpart[0] + "," + evalpart[1] + "," + evalpart[2] + ")";
                    return vector_three;
            }

            if (functions.ContainsKey(op))
            {
                var func = functions[op];
                var localVars = new Dictionary<string, Variable>();
                for (int j = 0; j < func.Parameters.Count; j++)
                {
                    string val = EvaluateExpression(parts[j].Trim(), scope);
                    localVars[func.Parameters[j].Name] = new Variable { Type = func.Parameters[j].Type, Value = val };
                }
                string result = ExecuteFunctionBody(func.Body, localVars);
                return result;
            }
        }
        return exprInput;
    }

    static string[] SplitArgs(string input)
    {
        List<string> args = new List<string>();
        int depth = 0, start = 0;
        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == '(') depth++;
            else if (input[i] == ')') depth--;
            else if (input[i] == ',' && depth == 0)
            {
                args.Add(input.Substring(start, i - start).Trim());
                start = i + 1;
            }
        }
        args.Add(input.Substring(start).Trim());
        return args.ToArray();
    }

    static void SetVariable(string name, string type, string value, Dictionary<string, Variable>? scope = null)
    {
        if (scope != null)
            scope[name] = new Variable { Type = type, Value = value };
        else
            variables[name] = new Variable { Type = type, Value = value };
    }

}
