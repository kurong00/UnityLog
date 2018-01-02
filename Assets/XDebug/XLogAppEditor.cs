using System.Collections.Generic;
using System;
using UnityEngine;

class XLogAppEditor : MonoBehaviour, ILogger
{
    public GUISkin Skin;
    public int FontSize = 0;

    List<LogInformation> LogList = new List<LogInformation>();
    int Errors = 0;
    int Warnings = 0;
    int Messages = 0;
    bool PauseOnError = false;
    public void Log(LogInformation log)
    {
        LogList.Add(log);
        switch (log.LogLevel)
        {
            case LogLevel.Error:
                Errors++;
                break;
            case LogLevel.Warning:
                Warnings++;
                break;
            default:
                Messages++;
                break;
        }
        if (PauseOnError && log.LogLevel == LogLevel.Error)
            Debug.Break();
    }

    void Clear()
    {
        LogList.Clear();
        Errors = 0;
        Warnings = 0;
        Messages = 0;
    }
}
