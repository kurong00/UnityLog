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
    Texture2D ErrorIcon;
    Texture2D WarningIcon;
    Texture2D MessageIcon;
    GUIStyle LogStyle;
    Vector2 DrawPos;

    [SerializeField]
    XLoggerEditor EditorLog;
    
    void OnEnable()
    {
        titleContent.text = "XLogConsole";
        if (!EditorLog)
        {
            EditorLog = XLogger.GetLogger<XLoggerEditor>();
            if (!EditorLog)
            {
                EditorLog = XLoggerEditor.Ctor();
            }
        }
        XLogger.AddLogger(EditorLog);
        EditorLog.AddWindow(this);

        ///Use Unity Editor Built-in Icons
        ///http://unitylist.com/r/dba/unity-editor-icons
        ErrorIcon = EditorGUIUtility.FindTexture("sv_icon_name6");
        WarningIcon = EditorGUIUtility.FindTexture("sv_icon_name4");
        MessageIcon = EditorGUIUtility.FindTexture("sv_icon_name0");
        DrawPos = Vector2.zero;
        Repaint();
    }
    public void OnGUI()
    {
        ///Use Default GUIStyle
        ///https://gist.github.com/MadLittleMods/ea3e7076f0f59a702ecb
        LogStyle = new GUIStyle("CN StatusWarn");
        LogStyle.fixedHeight = XLogGUIConstans.XLOG_DETAIL_LINE_HEIGHT;
        OnGUIToolBar();

    }
    void OnGUIToolBar()
    {
        var toolbarStyle = EditorStyles.toolbarButton;
        Vector2 btnSize;
        if (XLogGUIFunc.AdaptingButton(XLogGUIConstans.XLOG_TOOLBAR_BUTTON_CLEAR, 
            EditorStyles.toolbarButton,DrawPos, out btnSize))
        {
            EditorLog.ClearHistoryLogs();
        }
    }
    public void LogWindow(LogInformation log)
    {
        ///TODO
    }

    
}
