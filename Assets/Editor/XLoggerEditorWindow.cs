using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class XLoggerEditorWindow : EditorWindow, ILoggerWindow
{
    [MenuItem("Window/XLogConsole")]
    static void Init()
    {
        XLoggerEditorWindow xLoggerEditorWindow = (XLoggerEditorWindow)
            EditorWindow.GetWindow(typeof(XLoggerEditorWindow));
        xLoggerEditorWindow.Show();
    }
    
    void OnEnable()
    {
        titleContent.text = "XLogConsole";
    }
    public void LogWindow(LogInformation log)
    {
        ///TODO
    }
}
