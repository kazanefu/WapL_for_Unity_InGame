using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.IO;


abstract record VariableValue;

record I32Value(int Data) : VariableValue;
record F32Value(float Data) : VariableValue;
record I64Value(long Data) : VariableValue;
record F64Value(double Data) : VariableValue;
record StringValue(string Data) : VariableValue;
record BoolValue(bool Data) : VariableValue;
record Vec3Value(Vector3 Data) : VariableValue;
record NullableValue(string Data) : VariableValue;


class Variable
{
    public string Type; // "i32","f32", "String", "bool", "vec3", "gameobject", "component"
    public VariableValue Value;
}

class Function
{
    public List<(string Type, string Name)> Parameters = new List<(string, string)>();
    public List<string> Body = new List<string>();
}

public class WapLInterpreter : MonoBehaviour
{
    Dictionary<string, Variable> variables = new Dictionary<string, Variable>();
    Dictionary<string, Function> functions = new Dictionary<string, Function>();
    Dictionary<string, int> labelPositions = new Dictionary<string, int>();
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

        // ラベル位置のスキャン
        for (int i = 0; i < commands.Length; i++)
        {
            string line = commands[i].Trim();
            if (line.StartsWith("point "))
            {
                string labelName = line.Substring(6).Trim();
                labelPositions[labelName] = i;
            }
        }
        //関数のスキャン
        for (int i = 0; i < commands.Length; i++)
        {
            string trimmed = commands[i].Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) continue;

            if (trimmed.StartsWith("fn "))
            {
                string head = trimmed.Substring(3);
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
                string head = trimmed.Substring(3);
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
                    //Console.WriteLine("ラベルが見つかりません: " + labelName);
                    output += "\nラベルが見つかりません: " + labelName;
                    outputfield.text = output;
                }
            }

            // warptoif(条件, ラベル名) の処理
            if (trimmed.StartsWith("warptoif("))
            {
                string inner = trimmed.Substring(9, trimmed.Length - 10);
                string[] parts = SplitArgs(inner);
                if (parts.Length == 2)
                {
                    bool conditionResult = false;
                    switch (EvaluateExpression(parts[0].Trim()))
                    {
                        case BoolValue(var b): conditionResult = b;break;
                    }
                    
                    string label = parts[1].Trim();
                    if (conditionResult == true && labelPositions.ContainsKey(label))
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

    VariableValue EvaluateCommand(string line, Dictionary<string, Variable>? localScope = null)
    {
        if (line.Contains("(") && line.EndsWith(")"))
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
                    VariableValue val = EvaluateExpression(arguments[j].Trim(), localScope);
                    string type = func.Parameters[j].Type;
                    localVars[func.Parameters[j].Name] = new Variable { Type = type, Value = val };
                }

                VariableValue result = ExecuteFunctionBody(func.Body, localVars);
                //return result;
            }
            else
            {
                //EvaluateExpression(line, localScope);
            }
        }
        return EvaluateExpression(line, localScope);
    }
    VariableValue ExecuteFunctionBody(List<string> body, Dictionary<string, Variable> scope)
    {
        Debug.Log("Called");
        // 関数内だけのラベル表
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
                    //Console.WriteLine("関数内のラベルが見つかりません: " + labelName);
                    output += "関数内のラベルが見つかりません: " + labelName;
                    outputfield.text = output;
                }
            }

            if (line.StartsWith("warptoif("))
            {
                string inner = line.Substring(9, line.Length - 10);
                string[] parts = SplitArgs(inner);
                if (parts.Length == 2)
                {
                    bool condition = false;
                    switch(EvaluateExpression(parts[0].Trim(), scope))
                    {
                        case BoolValue(var b):condition = b;break;
                    }
                    string label = parts[1].Trim();
                    if (condition == true && localLabels.ContainsKey(label))
                    {
                        i = localLabels[label];
                        continue;
                    }
                }
            }


            if (line.StartsWith("return "))
            {
                string retExpr = line.Substring(7).Trim();
                return EvaluateExpression(retExpr, scope); // ← ここで値を返す
            }

            EvaluateCommand(line, scope);
        }
        return new NullableValue("");
    }
    VariableValue EvaluateExpression(string exprInput, Dictionary<string, Variable>? scope = null)
    {
        exprInput = exprInput.Trim();
        if (exprInput.StartsWith("\"") && exprInput.EndsWith("\"")) return new StringValue(exprInput.Substring(1, exprInput.Length - 2));
        if (double.TryParse(exprInput, out double n)) return new F64Value(n);
        if (exprInput == "true") { return new BoolValue(true); }else if(exprInput == "false") { return new BoolValue(false); }
        if ((scope != null && scope.ContainsKey(exprInput))) return scope[exprInput].Value;
        if (variables.ContainsKey(exprInput)) return variables[exprInput].Value;

        if (exprInput.Contains("(") && exprInput.EndsWith(")"))
        {
            int lparen = exprInput.IndexOf('(');
            string op = exprInput.Substring(0, lparen);
            string inside = exprInput.Substring(lparen + 1, exprInput.Length - lparen - 2);
            string[] parts = SplitArgs(inside);

            List<VariableValue> evalpart = new List<VariableValue>(parts.Length);
            if (op != "do")
            {
                for (int l = 0; l < parts.Length; l++)
                {
                    evalpart.Add(parts.Length > l ? EvaluateExpression(parts[l], scope) : new NullableValue(""));
                }
            }

            if (variables.ContainsKey(op) && variables[op].Type == "vec3")
            {
                VariableValue exprInputV3 = variables[op].Value;
                Vector3 parts3 = new Vector3(0f,0f,0f);
                switch (exprInputV3)
                {
                    case Vec3Value(var v):parts3 = v;break;
                }
                //int lparenV3 = exprInputV3.IndexOf('(');
                //string opV3 = exprInputV3.Substring(0, lparenV3);
                //string insideV3 = exprInput.Substring(lparenV3 + 1, exprInputV3.Length - lparenV3 - 2);
                //string[] partsV3 = SplitArgs(insideV3);
                if (parts[0] == "x")
                {
                    return new F32Value(parts3.x);
                }
                else if (parts[0] == "y")
                {
                    return new F32Value(parts3.y);
                }
                else if (parts[0] == "z")
                {
                    return new F32Value(parts3.z);
                }
            }

            switch (op)
            {
                case "+": return TypeAjust(TypeReturn(evalpart[0]),new F64Value(VariableToDouble(evalpart[0]) + VariableToDouble(evalpart[1])));
                //case "t+": return (evalpart[0] + evalpart[1]).ToString();
                case "-": return TypeAjust(TypeReturn(evalpart[0]), new F64Value(VariableToDouble(evalpart[0]) - VariableToDouble(evalpart[1])));
                case "*": return TypeAjust(TypeReturn(evalpart[0]), new F64Value(VariableToDouble(evalpart[0]) * VariableToDouble(evalpart[1])));
                //case "t*": string textadd = ""; for (int i = 1; i <= int.Parse(evalpart[1]); i++) { textadd += evalpart[0]; } return textadd;
                case "/": return VariableToDouble(evalpart[1]) != 0 ? TypeAjust(TypeReturn(evalpart[0]), new F64Value(VariableToDouble(evalpart[0]) / VariableToDouble(evalpart[1]))) : TypeAjust(TypeReturn(evalpart[0]),new F64Value(0));
                case "%": return VariableToDouble(evalpart[1]) != 0 ? TypeAjust(TypeReturn(evalpart[0]), new F64Value(VariableToDouble(evalpart[0]) % VariableToDouble(evalpart[1]))) : TypeAjust(TypeReturn(evalpart[0]), new F64Value(0));
                case "==": return new BoolValue(evalpart[0] == evalpart[1]);
                case "!=": return new BoolValue(!(evalpart[0] == evalpart[1]));
                case ">": return new BoolValue(VariableToDouble(evalpart[0]) > VariableToDouble(evalpart[1]));
                case "<": return new BoolValue(VariableToDouble(evalpart[0]) < VariableToDouble(evalpart[1]));
                case ">=": return new BoolValue(VariableToDouble(evalpart[0]) >= VariableToDouble(evalpart[1]));
                case "<=": return new BoolValue(VariableToDouble(evalpart[0]) <= VariableToDouble(evalpart[1]));
                case "and": return new BoolValue(VariableToBool(evalpart[0]) && VariableToBool(evalpart[1]));
                case "or": return new BoolValue(VariableToBool(evalpart[0]) || VariableToBool(evalpart[1]));
                case "not": return new BoolValue(!VariableToBool(evalpart[0]));
                case "+=": if ((scope != null && scope.ContainsKey(parts[0]))) { SetVariable(parts[0], scope[parts[0]].Type, new F64Value(VariableToDouble(evalpart[0]) + VariableToDouble(evalpart[1])), scope); } else if (variables.ContainsKey(parts[0])) { SetVariable(parts[0], variables[parts[0]].Type, new F64Value(VariableToDouble(evalpart[0]) + VariableToDouble(evalpart[1])), null); } return new F64Value(VariableToDouble(evalpart[0]) + VariableToDouble(evalpart[1]));
                case "-=": if ((scope != null && scope.ContainsKey(parts[0]))) { SetVariable(parts[0], scope[parts[0]].Type, new F64Value(VariableToDouble(evalpart[0]) - VariableToDouble(evalpart[1])), scope); } else if (variables.ContainsKey(parts[0])) { SetVariable(parts[0], variables[parts[0]].Type, new F64Value(VariableToDouble(evalpart[0]) - VariableToDouble(evalpart[1])), null); } return new F64Value(VariableToDouble(evalpart[0]) - VariableToDouble(evalpart[1]));
                case "*=": if ((scope != null && scope.ContainsKey(parts[0]))) { SetVariable(parts[0], scope[parts[0]].Type, new F64Value(VariableToDouble(evalpart[0]) * VariableToDouble(evalpart[1])), scope); } else if (variables.ContainsKey(parts[0])) { SetVariable(parts[0], variables[parts[0]].Type, new F64Value(VariableToDouble(evalpart[0]) * VariableToDouble(evalpart[1])), null); } return new F64Value(VariableToDouble(evalpart[0]) * VariableToDouble(evalpart[1]));
                case "/=": if (VariableToDouble(evalpart[1]) != 0) { if ((scope != null && scope.ContainsKey(parts[0]))) { SetVariable(parts[0], scope[parts[0]].Type, new F64Value(VariableToDouble(evalpart[0]) / VariableToDouble(evalpart[1])), scope); } else if (variables.ContainsKey(parts[0])) { SetVariable(parts[0], variables[parts[0]].Type, new F64Value(VariableToDouble(evalpart[0]) / VariableToDouble(evalpart[1])), null); } return new F64Value(VariableToDouble(evalpart[0]) / VariableToDouble(evalpart[1])); } return new F64Value(0);
                case "%=": if (VariableToDouble(evalpart[1]) != 0) { if ((scope != null && scope.ContainsKey(parts[0]))) { SetVariable(parts[0], scope[parts[0]].Type, new F64Value(VariableToDouble(evalpart[0]) % VariableToDouble(evalpart[1])), scope); } else if (variables.ContainsKey(parts[0])) { SetVariable(parts[0], variables[parts[0]].Type, new F64Value(VariableToDouble(evalpart[0]) % VariableToDouble(evalpart[1])), null); } return new F64Value(VariableToDouble(evalpart[0]) % VariableToDouble(evalpart[1])); } return new F64Value(0);
                case "=":
                    string type = "String";
                    if (parts.Length < 3) { type = TypeReturn(evalpart[0]); } else { type = VariableToString(evalpart[2]); }
                    string name = parts[0].Trim();  // parts[0]
                    VariableValue value = evalpart[1]; // parts[1]
                    SetVariable(name, type, value, scope);
                    return value;
                    //if (type == "number" || type == "bool" || type == "vec3")
                    //{
                    //    SetVariable(parts[1], type, value, scope);
                    //    return value;
                    //}
                    //else if (type == "text")
                    //{
                    //    // 文字列なら囲みを除去
                    //    if (value.StartsWith("\"") && value.EndsWith("\""))
                    //    {
                    //        value = value.Substring(1, value.Length - 2);
                    //    }
                    //    SetVariable(name, type, value, scope);
                    //    return value;
                    //}
                    //return new VariableValue { Type = "", I32 = 0, F32 = 0f, Str = "", Bool = false, Vec3 = new Vector3(0f, 0f, 0f) };
                case "input":
                    //ゲーム内でコンソールの入力は受けないので廃止
                    //string input_name = evalpart[0];
                    //Console.Write($"入力 [{input_name}]: ");
                    //string input_value = Console.ReadLine() ?? "";
                    //return input_value;
                    return new StringValue(exprInput.Trim());
                case "print":
                    for (int i = 0; i <= parts.Length - 1; i++)
                    {
                        //Console.WriteLine(evalpart[i]);
                        Debug.Log(VariableToString(To_String(evalpart[i])));
                        output += "\n" + VariableToString(To_String(evalpart[i]));
                        outputfield.text = output;
                    }

                    return evalpart[0];
                case "if":
                    if (VariableToBool(evalpart[0]) == true) { return evalpart[1]; } else { return evalpart[2]; }
                case "do":
                    var localVars = new Dictionary<string, Variable>();
                    List<string> todo = new List<string>();
                    for (int i = 0; i <= parts.Length - 1; i++)
                    {
                        todo.Add(parts[i].Trim());
                    }

                    VariableValue result = ExecuteFunctionBody(todo, localVars);
                    return result;
                case "vec3":
                    Vec3Value vector_three = new Vec3Value(new Vector3(VariableToFloat(evalpart[0]), VariableToFloat(evalpart[1]), VariableToFloat(evalpart[2])));
                    return vector_three;
            }

            if (functions.ContainsKey(op))
            {
                Debug.Log("find function");
                var func = functions[op];
                var localVars = new Dictionary<string, Variable>();
                for (int j = 0; j < func.Parameters.Count; j++)
                {
                    VariableValue val = EvaluateExpression(parts[j].Trim(), scope);
                    localVars[func.Parameters[j].Name] = new Variable { Type = func.Parameters[j].Type, Value = val };
                }
                VariableValue result = ExecuteFunctionBody(func.Body, localVars);
                return result;
            }
        }
        return new StringValue(exprInput.Trim());
    }

    string[] SplitArgs(string input)
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

    void SetVariable(string name, string type, VariableValue value, Dictionary<string, Variable>? scope = null)
    {

        VariableValue value_new = TypeAjust(type,value);
        if (scope != null)
            scope[name] = new Variable { Type = type, Value = value_new };
        else
            variables[name] = new Variable { Type = type, Value = value_new };
    }

    double VariableToDouble(VariableValue value)
    {
        double val = 0.0;
        switch (value)
        {
            case I32Value(var i):val = i;break;
            case I64Value(var l): val = l; break;
            case F32Value(var f): val = f; break;
            case F64Value(var d): val = d; break;
        }
        return val;
    }

    float VariableToFloat(VariableValue value)
    {
        float val = 0.0f;
        switch (value)
        {
            case I32Value(var i): val = i; break;
            case I64Value(var l): val = l; break;
            case F32Value(var f): val = f; break;
            case F64Value(var d): val = (float)d; break;
        }
        return val;
    }

    VariableValue TypeAjust(string type, VariableValue value)
    {
        double val = 0;
        string s_val = "";
        bool b_val = false;
        Vector3 v3_val = Vector3.zero;
        switch (value)
        {
            case F64Value(var d):val = d;break;
            case F32Value(var f): val = f; break;
            case I64Value(var l): val = l; break;
            case I32Value(var i): val = i; break;
            case StringValue(var s): s_val = s; break;
            case BoolValue(var b): b_val = b; break;
            case Vec3Value(var v): v3_val = v; break;
        }
        switch (type)
        {
            case "i32":return new I32Value((int)val);
            case "i64": return new I64Value((long)val);
            case "f32": return new F32Value((float)val);
            case "f64": return new F64Value(val);
            case "String": return new StringValue(s_val);
            case "bool": return new BoolValue(b_val);
            case "vec3": return new Vec3Value(v3_val);
            default: return value;
        }
    }
    string TypeReturn(VariableValue value)
    {
        string ret = "i32";
        switch (value)
        {
            case I32Value(var i): ret = "i32"; break;
            case I64Value(var l): ret = "i64"; break;
            case F32Value(var f): ret = "f32"; break;
            case F64Value(var d): ret = "f64"; break;
            case StringValue(var s): ret = "String"; break;
            case BoolValue(var b): ret = "bool"; break;
            case Vec3Value(var v3): ret = "Vec3"; break;
        }
        return ret;
    }
    bool VariableToBool(VariableValue value)
    {
        bool val = false;
        switch (value)
        {
            case BoolValue(var b):val = b;break;
        }
        return val;
    }
    string VariableToString(VariableValue value)
    {
        string val = "";
        switch (value)
        {
            case StringValue(var b): val = b; break;
        }
        return val;
    }
    VariableValue To_String(VariableValue value)
    {
        string s_val = "";
        switch (value)
        {
            case F64Value(var d): s_val = d.ToString(); break;
            case F32Value(var f): s_val = f.ToString(); break;
            case I64Value(var l): s_val = l.ToString(); break;
            case I32Value(var i): s_val = i.ToString(); break;
            case StringValue(var s): s_val = s; break;
            case BoolValue(var b): s_val = b.ToString(); break;
        }
        return new StringValue(s_val);
    }

}
