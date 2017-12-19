﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LogLevel
{
    Message,
    Warning,
    Error,
}
public class LogInformation {

    List<LogStackFrame> FrameStackList;
    string Message;
    double RelativeTimeLine;
    double AbsoluteTimeLine;
    LogLevel logLevel;
}