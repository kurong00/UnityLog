using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

[Serializable]
public class LogStackFrame  {

    public string MethodName;
    public string DeclaringType;
    public StringBuilder ParameterMsg;

    public int LineNumber;
    public string FileName;

    string formatMethodNameByFile;
    public string FormatMethodNameByFile
    {
        get
        {
            return formatMethodNameByFile;
        }
        set
        {
            formatMethodNameByFile = value;
        }
    }
    string formatMethodName;
    public string FormatMethodName
    {
        get
        {
            return formatMethodName;
        }
        set
        {
            formatMethodName = value;
        }
    }
    string formatFileName;
    public string FormatFileName
    {
        get
        {
            return formatFileName;
        }
        set
        {
            formatFileName = value;
        }
    }

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
        FormatNames();
    }

    public LogStackFrame(string message,string filename,int linenumber)
    {
        FileName = filename;
        LineNumber = linenumber;
        ParameterMsg.Append(message);
    }

    public LogStackFrame(string unityStackFrame)
    {
        var regex = Regex.Matches(unityStackFrame, @"(.*)\.(.*)\s*\(.*\(at (.*):(\d+)");
        if (regex.Count > 0)
        {
            DeclaringType = regex[0].Groups[1].Value;
            MethodName = regex[0].Groups[2].Value;
            FileName = regex[0].Groups[3].Value;
            LineNumber = Convert.ToInt32(regex[0].Groups[4].Value);
            FormatNames();
        }
        else
        {
            FormatFileName = unityStackFrame;
            FormatMethodName = unityStackFrame;
            FormatMethodNameByFile = unityStackFrame;
        }
    }

    void FormatNames()
    {
        FormatMethodName = string.Format("{0}:{1}({2})", DeclaringType, MethodName, ParameterMsg);
        string tempFileName = FileName;
        if (!string.IsNullOrEmpty(tempFileName))
        {
            int index = FileName.IndexOf("Assets", StringComparison.OrdinalIgnoreCase);
            if(index > 0)
            {
                tempFileName = FileName.Substring(index);
            }
        }
        FormatFileName = string.Format("{0}:{1}", tempFileName, LineNumber);
        FormatMethodNameByFile = string.Format("{0} : from {1}", FormatMethodName, formatFileName);
    }
    
    /*bool GetInformationFromUnityStackFrame(string unityLog,ref string methodName,ref string declaringType,
        ref string fileName,ref int lineNumber)
    {
        var regex = Regex.Matches(unityLog, @"(.*)\.(.*)\s*\(.*\(at (.*):(\d+)");
        if (regex.Count > 0)
        {
            declaringType = regex[0].Groups[1].Value;
            methodName = regex[0].Groups[2].Value;
            fileName = regex[0].Groups[3].Value;
            lineNumber = Convert.ToInt32(regex[0].Groups[4].Value);
            return true;
        }
        else
            return false;
    }*/
}
