using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;

[Serializable]
public class LogStackFrame  {

    public string MethodName;
    public string DeclaringType;
    public int LineNumber;
    public string FileName;

    public LogStackFrame(StackFrame stackFrame)
    {
        var method = stackFrame.GetMethod();
        MethodName = method.Name;

    }
}
