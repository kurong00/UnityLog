using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Diagnostics;

[Serializable]
public class LogStackFrame  {

    public string MethodName;
    public string DeclaringType;
    public StringBuilder ParameterMsg;

    public int LineNumber;
    public string FileName;

    public LogStackFrame(StackFrame stackFrame)
    {
        var method = stackFrame.GetMethod();
        MethodName = method.Name;
        DeclaringType = method.DeclaringType.FullName;
        var parameters = method.GetParameters();
        ParameterMsg = new StringBuilder();
        for (int i = 0; i < parameters.Length; i++)
        {
            ParameterMsg.Append(string.Format("{0},{1}", parameters[i].ParameterType, parameters[i].Name));
            if (i < parameters.Length - 1)
                ParameterMsg.Append(", ");
        }
        FileName = stackFrame.GetFileName();
        LineNumber = stackFrame.GetFileLineNumber();
    }
}
