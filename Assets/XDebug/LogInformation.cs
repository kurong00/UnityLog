using UnityEngine;
using System.Collections.Generic;
using System;
public enum LogLevel
{
    Message,
    Warning,
    Error,
}
[Serializable]
public class LogInformation
{
    UnityEngine.Object OriginObject;
    string Channel;
    List<LogStackFrame> StackFrameList;
    LogStackFrame OriginStackFrame;
    LogLevel LogLevel;

    string Message;
    double RelativeTimeLine;
    double AbsoluteTimeLine;

    public LogInformation(UnityEngine.Object origin,string channel,LogLevel level,
        List<LogStackFrame> stackFrameList,LogStackFrame logStackFrame, object message)
    {
        OriginObject = origin;
        Channel = channel;
        LogLevel = level;
        StackFrameList = stackFrameList;
        var formatMessage = message as String;
        if(formatMessage != null)
        {
            Message = formatMessage;
        }
        OriginStackFrame = logStackFrame;
    }
}
