using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

public interface ILogger
{
    void Log(LogInformation log);
}

public interface IFilter
{

}

[AttributeUsage(AttributeTargets.Method)]
public class ExcludeStackTrace : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class OnlyUnityLog : Attribute { }
public static class XLogger
{
    public static int MaxMessage = 500;
    public static bool UseBothSystem = true;
    public static string UnityNewLine = "/n";
    public static char DirectorySeparator = '/';

    static List<ILogger> LoggerList = new List<ILogger>();
    static List<IFilter> FilterList = new List<IFilter>();
    static long StartTimeTicks;
    static bool Logged;
    static Regex MessageRegex;

    static XLogger()
    {

    }

    [ExcludeStackTrace]
    static void UnityLogHandler(string unityLogMessage, string unityStackFrame, LogType logType)
    {
        lock (LoggerList)
        {
            if (!Logged)
            {
                Logged = true;
                LogLevel logLevel;
                switch (logType)
                {
                    case LogType.Warning:
                        logLevel = LogLevel.Warning;
                        break;
                    case LogType.Assert:
                        logLevel = LogLevel.Error;
                        break;
                    case LogType.Error:
                        logLevel = LogLevel.Error;
                        break;
                    case LogType.Exception:
                        logLevel = LogLevel.Error;
                        break;
                    default:
                        logLevel = LogLevel.Message;
                        break;
                }
                LogStackFrame orginStackFrame;
                List<LogStackFrame> stackFrames = new List<LogStackFrame>();
                if(stackFrames.Count == 0)
                {
                    stackFrames = GetStackFrameFromeUnity(unityStackFrame, out orginStackFrame);
                }
            }
        }
    }

    static List<LogStackFrame> GetStackFrameFromeUnity(string unityStackFrame,out LogStackFrame orginStackFrame)
    {
        var newLines = Regex.Split(unityStackFrame, UnityNewLine);
        List<LogStackFrame> stackFrames = new List<LogStackFrame>();
        foreach(var line in newLines)
        {
            var frame = new LogStackFrame(line);
            if (!string.IsNullOrEmpty(frame.FormatMethodNameByFile))
            {
                //change!
                stackFrames.Add(frame);
            }
        }
        if (stackFrames.Count > 0)
            orginStackFrame = stackFrames[0];
        else
            orginStackFrame = null;
        return stackFrames;
    }
}
