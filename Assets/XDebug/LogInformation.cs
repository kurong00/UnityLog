using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LogLevel
{
    Message,
    Warning,
    Error,
}
public class LogInformation
{

    Object Origin;
    string Channel;
    List<LogStackFrame> StackFrameList;
    LogLevel logLevel;

    string Message;
    double RelativeTimeLine;
    double AbsoluteTimeLine;
    
}
