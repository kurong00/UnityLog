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

    public LogStackFrame(StackFrame stackFrame)
    {
        var name = stackFrame.GetMethod();
    }
}
