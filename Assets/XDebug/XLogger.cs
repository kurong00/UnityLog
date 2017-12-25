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
    static void UnityLogHandler(string logString, string stackTrace, LogType logType)
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
            }
        }
    }
}
