using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEditor;

[System.Serializable]
public class XLoggerEditor : ScriptableObject, ILogger
{
    List<LogInformation> LogInformationList = new List<LogInformation>();
    List<ILoggerWindow> Windows = new List<ILoggerWindow>();
    HashSet<string> Channels = new HashSet<string>();

    public bool ErrorPause;
    public bool ClearOnPlay;
    public bool Playing;
    public int Errors;
    public int Warnings;
    public int Messages;

    static public XLoggerEditor Create()
    {
        XLoggerEditor xLoggerEditor = ScriptableObject.FindObjectOfType<XLoggerEditor>();
        if (xLoggerEditor == null)
            xLoggerEditor = ScriptableObject.CreateInstance<XLoggerEditor>();
        xLoggerEditor.Errors = 0;
        xLoggerEditor.Warnings = 0;
        xLoggerEditor.Messages = 0;
        xLoggerEditor.ErrorPause = false;
        xLoggerEditor.ClearOnPlay = true;
        xLoggerEditor.Playing = false;
        return xLoggerEditor;
    }

    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += ClearLogsOnPlay;

    }
    void ClearLogsOnPlay(PlayModeStateChange state)
    {
        ///TODO
        if (!Playing && EditorApplication.isPlayingOrWillChangePlaymode && ClearOnPlay)
            ClearHistoryLogs();
        Playing = EditorApplication.isPlayingOrWillChangePlaymode;
    }

    void ClearHistoryLogs()
    {
        LogInformationList.Clear();
        Channels.Clear();
        Errors = 0;
        Warnings = 0;
        Messages = 0;
        foreach (var window in Windows)
        {
            window.LogWindow(null);
        }
    }

    public void Log(LogInformation log)
    {
        lock (this)
        {
            if (!String.IsNullOrEmpty(log.Channel) && !Channels.Contains(log.Channel))
            {
                Channels.Add(log.Channel);
            }

            LogInformationList.Add(log);
        }

        if (log.LogLevel == LogLevel.Error)
        {
            Errors++;
        }
        else if (log.LogLevel == LogLevel.Warning)
        {
            Warnings++;
        }
        else
        {
            Messages++;
        }

        foreach (var window in Windows)
        {
            window.LogWindow(log);
        }

        if (log.LogLevel == LogLevel.Error && ErrorPause)
        {
            Debug.Break();
        }
    }

    public void AddWindow(ILoggerWindow window)
    {
        if (!Windows.Contains(window))
        {
            Windows.Add(window);
        }
    }
}
