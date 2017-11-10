using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;

[Serializable]
public class ULogStackFrame  {

    public string MethodName;
    public string DeclaringType;
    public int LineNumber;

    public ULogStackFrame(StackFrame stackFrame)
    {
        var name = stackFrame.GetMethod();
    }
}
