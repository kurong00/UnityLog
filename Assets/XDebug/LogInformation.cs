using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using System;
using System.Reflection;
using System.Text.RegularExpressions;

public enum LogLevel
{
    Message,
    Warning,
    Error,
}
[Serializable]
public class LogInformation
{
    UnityEngine.Object Origin;
    string Channel;
    List<LogStackFrame> StackFrameList;
    LogStackFrame originStackFrame;
    LogLevel logLevel;

    string Message;
    double RelativeTimeLine;
    double AbsoluteTimeLine;

    public LogInformation(UnityEngine.Object origin,string channel,LogLevel level,
        List<LogStackFrame> stackFrameList,LogStackFrame logStackFrame, System.Object message)
    {
        Origin = origin;
        Channel = channel;
        logLevel = level;
        StackFrameList = stackFrameList;
        originStackFrame = logStackFrame;
        var formatMessage = message as String;
        if(formatMessage != null)
        {
            Message = formatMessage;
        }
        originStackFrame = logStackFrame;
    }
}
