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
    public UnityEngine.Object OriginObject;
    public string Channel;
    public List<LogStackFrame> StackFrameList;
    LogStackFrame OriginStackFrame;
    public LogLevel LogLevel;

    public string Message;
    double relativeTimeLine;
    public double RelativeTimeLine
    {
        get
        {
            return relativeTimeLine;
        }
        set
        {
            relativeTimeLine = value;
        }
    }
    double AbsoluteTimeLine;

    public LogInformation(UnityEngine.Object origin,string channel,LogLevel level,List<LogStackFrame> stackFrameList,
        LogStackFrame logStackFrame, object message, params object[] paramsObject)
    {
        OriginObject = origin;
        Channel = channel;
        LogLevel = level;
        StackFrameList = stackFrameList;
        var formatMessage = message as String;
        if(formatMessage != null)
        {
            if (paramsObject.Length > 0)
            {
                Message = System.String.Format(formatMessage, paramsObject);
            }
            else
            {
                Message = formatMessage;
            }
        }
        else
        {
            if (message != null)
                Message = message.ToString();
        }
        RelativeTimeLine = XLogger.GetRelativeTime();
        OriginStackFrame = logStackFrame;
    }
    

}
