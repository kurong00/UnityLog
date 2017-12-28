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

public interface ILoggerWindow
{
    void LogWindow(LogInformation log);
}

public interface IFilter
{
    bool ApplyFilter(UnityEngine.Object origin, LogLevel logLevel,
        string channel, System.Object message, params object[] paramsObject);
}

[AttributeUsage(AttributeTargets.Method)]
public class ExcludeStackTrace : Attribute { }

[AttributeUsage(AttributeTargets.Method)]
public class OnlyUnityLog : Attribute { }
public static class XLogger
{
    public static int MaxMessage = 500;
    public static bool UseBothSystem = false;
    public static string UnityNewLine = "/n";
    public static string DirectorySeparator = "/";

    static List<ILogger> LoggerList = new List<ILogger>();
    static List<IFilter> FilterList = new List<IFilter>();
    static LinkedList<LogInformation> RecentMessages = new LinkedList<LogInformation>();
    static long StartTimeTicks;
    static bool Logged;
    static Regex MessageRegex;

    static XLogger()
    {
        Application.logMessageReceivedThreaded += UnityLogHandler;
        StartTimeTicks = DateTime.Now.Ticks;
        MessageRegex = new Regex(@"(.*)\((\d+).*\)");
    }

    [ExcludeStackTrace]
    static void UnityLogHandler(string unityLogMessage, string unityStackFrame, LogType logType)
    {
        lock (LoggerList)
        {
            if (!Logged)
            {
                try
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
                    bool OnlyUnityLog = GetStackFramListFromUnity(ref stackFrames, out orginStackFrame);
                    if (OnlyUnityLog)
                        return;
                    if (stackFrames.Count == 0)
                        stackFrames = GetStackFrameFromeUnity(unityStackFrame, out orginStackFrame);
                    string fileName = "";
                    int lineNumber = 0;
                    if (ExtractInformationFromUnityLog(unityLogMessage, ref fileName, ref lineNumber))
                    {
                        stackFrames.Insert(0, new LogStackFrame(unityLogMessage, fileName, lineNumber));
                    }
                    var logInformation = new LogInformation(null, "", logLevel, stackFrames,
                        orginStackFrame, unityLogMessage);
                    RecentMessages.AddLast(logInformation);
                    while (RecentMessages.Count > MaxMessage)
                    {
                        RecentMessages.RemoveFirst();
                    }
                    ///TODO
                    ///
                    ///LoggerList.RemoveAll(l => l == null);
                    ///LoggerList.ForEach(l => l.Log(logInformation));
                    ///
                    foreach (ILogger logs in LoggerList)
                    {
                        if (logs == null)
                            LoggerList.Remove(logs);
                        else
                            logs.Log(logInformation);
                    }
                }
                finally
                {
                    Logged = false;
                }
            }
        }
    }

    [ExcludeStackTrace]
    static public void Log(UnityEngine.Object origin, LogLevel logLevel,
        string channel, object message, params object[] paramsObject)
    {
        lock (LoggerList)
        {
            if (!Logged)
            {
                try
                {
                    Logged = true;
                    foreach (IFilter filter in FilterList)
                    {
                        if (!filter.ApplyFilter(origin, logLevel, channel, message, paramsObject))
                            return;
                    }
                    LogStackFrame orginStackFrame;
                    List<LogStackFrame> stackFrames = new List<LogStackFrame>();
                    bool OnlyUnityLog = GetStackFramListFromUnity(ref stackFrames, out orginStackFrame);
                    if (OnlyUnityLog)
                        return;
                    var logInformation = new LogInformation(origin, channel, logLevel,
                        stackFrames, orginStackFrame, message);
                    RecentMessages.AddLast(logInformation);
                    while (RecentMessages.Count > MaxMessage)
                    {
                        RecentMessages.RemoveFirst();
                    }
                    foreach (ILogger logs in LoggerList)
                    {
                        if (logs == null)
                            LoggerList.Remove(logs);
                        else
                            logs.Log(logInformation);
                    }
                    if (UseBothSystem)
                    {
                        PushBackToUnity(origin, logLevel, message, paramsObject);
                    }
                }
                finally
                {
                    Logged = false;
                }
            }
        }
    }

    static List<LogStackFrame> GetStackFrameFromeUnity(string unityStackFrame, out LogStackFrame orginStackFrame)
    {
        var newLines = Regex.Split(unityStackFrame, UnityNewLine);
        List<LogStackFrame> stackFrames = new List<LogStackFrame>();
        foreach (var line in newLines)
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

    static bool GetStackFramListFromUnity(ref List<LogStackFrame> stackFrameList, out LogStackFrame orginStackFrame)
    {
        stackFrameList.Clear();
        orginStackFrame = null;
        StackTrace stackTrace = new StackTrace(true);
        StackFrame[] stackFrames = stackTrace.GetFrames();
        bool MeetFirstIgnoredMethod = false;
        for (int i = stackFrames.Length - 1; i > 0; i--)
        {
            StackFrame tempStackFrame = stackFrames[i];
            var method = tempStackFrame.GetMethod();
            if (method.IsDefined(typeof(OnlyUnityLog), true))
                return true;
            if (!method.IsDefined(typeof(ExcludeStackTrace), true))
            {
                UnityMethod.MethodMode mode = UnityMethod.GetMehodMode(method);
                bool isShowed;
                if (mode == UnityMethod.MethodMode.Show)
                    isShowed = true;
                else
                    isShowed = false;
                if (mode == UnityMethod.MethodMode.ShowFirst)
                {
                    if (!MeetFirstIgnoredMethod)
                    {
                        MeetFirstIgnoredMethod = true;
                        mode = UnityMethod.MethodMode.Show;
                    }
                    else
                        mode = UnityMethod.MethodMode.Hide;
                }
                if (mode == UnityMethod.MethodMode.Show)
                {
                    LogStackFrame logStackFrame = new LogStackFrame(tempStackFrame);
                    stackFrameList.Add(logStackFrame);
                    if (isShowed)
                        orginStackFrame = logStackFrame;
                }

            }
        }
        return false;
    }

    static public bool ExtractInformationFromUnityLog(string log, ref string fileName, ref int lineNumber)
    {
        var match = MessageRegex.Matches(log);
        if (match.Count > 0)
        {
            fileName = match[0].Groups[1].Value;
            lineNumber = Convert.ToInt32(match[0].Groups[2].Value);
            return true;
        }
        return false;
    }

    static public T GetLogger<T>() where T : class
    {
        foreach (var logger in LoggerList)
        {
            if (logger is T)
            {
                return logger as T;
            }
        }
        return null;
    }

    static public void AddLogger(ILogger logger, bool populateWithExistingMessages = true)
    {
        lock (LoggerList)
        {
            if (populateWithExistingMessages)
            {
                foreach (var oldLog in RecentMessages)
                {
                    logger.Log(oldLog);
                }
            }

            if (!LoggerList.Contains(logger))
            {
                LoggerList.Add(logger);
            }
        }
    }

    static void PushBackToUnity(UnityEngine.Object source, LogLevel severity, object message, params object[] paramsObject)
    {
        object showObject = null;
        if (message != null)
        {
            var messageAsString = message as string;
            if (messageAsString != null)
            {
                if (paramsObject.Length > 0)
                {
                    showObject = String.Format(messageAsString, paramsObject);
                }
                else
                {
                    showObject = message;
                }
            }
            else
            {
                showObject = message;
            }
        }
        if (source == null)
        {
            if (severity == LogLevel.Message)
                UnityEngine.Debug.Log(showObject);
            if (severity == LogLevel.Warning)
                UnityEngine.Debug.LogWarning(showObject);
            if (severity == LogLevel.Error)
                UnityEngine.Debug.LogError(showObject);
        }
        else
        {
            if (severity == LogLevel.Message)
                UnityEngine.Debug.Log(showObject, source);
            if (severity == LogLevel.Warning)
                UnityEngine.Debug.LogWarning(showObject, source);
            if (severity == LogLevel.Error)
                UnityEngine.Debug.LogError(showObject, source);
        }

    }

    static public double GetRelativeTime()
    {
        long ticks = DateTime.Now.Ticks;
        return TimeSpan.FromTicks(ticks - StartTimeTicks).TotalSeconds;
    }
}