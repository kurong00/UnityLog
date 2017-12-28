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
    bool ShowTimes;
    bool Collapse;
    bool DirtyLog;
    bool ScrowDown;
    bool ShowErrors;
    bool ShowWarnings;
    bool ShowMessage;
    string FilterRegex;

    [SerializeField]
    XLoggerEditor EditorLog;

    void OnEnable()
    {
        titleContent.text = XLogGUIConstans.XLOG_EDITOR_NAME;
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
        ErrorIcon = EditorGUIUtility.FindTexture(XLogGUIConstans.XLOG_ICON_ERROR);
        WarningIcon = EditorGUIUtility.FindTexture(XLogGUIConstans.XLOG_ICON_WARNING);
        MessageIcon = EditorGUIUtility.FindTexture(XLogGUIConstans.XLOG_ICON_MESSAGE);
        ShowTimes = true;
        Collapse = false;
        DirtyLog = false;
        ScrowDown = false;
        ShowErrors = true;
        ShowWarnings = true;
        ShowMessage = true;
        DrawPos = Vector2.zero;
        Repaint();
    }
    public void OnGUI()
    {
        ///Use Default GUIStyle
        ///https://gist.github.com/MadLittleMods/ea3e7076f0f59a702ecb
        LogStyle = new GUIStyle("CN StatusWarn");
        LogStyle.fixedHeight = XLogGUIConstans.XLOG_DETAIL_LINE_HEIGHT;
        GUILayout.BeginVertical();
        {
            OnGUIToolBar();
            OnGUIFilter();
        }
        GUILayout.EndVertical();
    }
    void OnGUIToolBar()
    {
        /*Vector2 btnSize;
        if (XLogGUIFunc.AdaptingButton(XLogGUIConstans.XLOG_TOOLBAR_BUTTON_CLEAR, 
            EditorStyles.toolbarButton,DrawPos, out btnSize))
            EditorLog.ClearHistoryLogs();
        DrawPos.x += btnSize.x;
        EditorLog.ClearOnPlay = XLogGUIFunc.AdaptingToggle(EditorLog.ClearOnPlay,
            XLogGUIConstans.XLOG_TOOLBAR_BUTTON_CLEAR_ON_PLAY, EditorStyles.toolbarButton, DrawPos,out btnSize);
        DrawPos.x += btnSize.x;
        EditorLog.ErrorPause = XLogGUIFunc.AdaptingToggle(EditorLog.ErrorPause,
            XLogGUIConstans.XLOG_TOOLBAR_BUTTON_ERROR_PAUSE, EditorStyles.toolbarButton,DrawPos,out btnSize);
        DrawPos.x += btnSize.x;
        DrawPos = Vector2.zero;*/
        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(XLogGUIConstans.XLOG_TOOLBAR_BUTTON_CLEAR, EditorStyles.toolbarButton))
                    EditorLog.ClearHistoryLogs();
                EditorLog.ClearOnPlay = GUILayout.Toggle(EditorLog.ClearOnPlay,
                    XLogGUIConstans.XLOG_TOOLBAR_TOGGLE_CLEAR_ON_PLAY, EditorStyles.toolbarButton);
                EditorLog.ErrorPause = GUILayout.Toggle(EditorLog.ErrorPause,
                    XLogGUIConstans.XLOG_TOOLBAR_TOGGLE_ERROR_PAUSE, EditorStyles.toolbarButton);
                bool showTimes = GUILayout.Toggle(ShowTimes, XLogGUIConstans.XLOG_TOOLBAR_TOGGLE_SHOW_TIMES,
                    EditorStyles.toolbarButton);
                ScrowDown = GUILayout.Toggle(ScrowDown, XLogGUIConstans.XLOG_TOOLBAR_TOGGLE_SCROW_DOWN,
                    EditorStyles.toolbarButton);
                if (showTimes != ShowTimes)
                {
                    DirtyLog = true;
                    ShowTimes = showTimes;
                }
                bool collapse = GUILayout.Toggle(ShowTimes, XLogGUIConstans.XLOG_TOOLBAR_TOGGLE_COLLAPSE,
                    EditorStyles.toolbarButton);
                if (collapse != Collapse)
                {
                    DirtyLog = true;
                    Collapse = collapse;
                }
            }
            GUILayout.EndHorizontal();

            GUIContent errorToggleContent = new GUIContent(EditorLog.Errors.ToString(), ErrorIcon);
            GUIContent warningToggleContent = new GUIContent(EditorLog.Warnings.ToString(), WarningIcon);
            GUIContent messageToggleContent = new GUIContent(EditorLog.Messages.ToString(), MessageIcon);
            float totalErrorButtonWidth = EditorStyles.toolbarButton.CalcSize(errorToggleContent).x
                + EditorStyles.toolbarButton.CalcSize(warningToggleContent).x +
                EditorStyles.toolbarButton.CalcSize(messageToggleContent).x;

            GUILayout.Space(position.width / 2 + totalErrorButtonWidth);

            GUILayout.BeginHorizontal();
            {
                GUILayout.Toggle(ShowErrors, errorToggleContent, EditorStyles.toolbarButton);
                GUILayout.Toggle(ShowWarnings, warningToggleContent, EditorStyles.toolbarButton);
                GUILayout.Toggle(ShowMessage, messageToggleContent, EditorStyles.toolbarButton);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndHorizontal();
    }

    void OnGUIFilter()
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Filter : ",GUILayout.Width(40));
            FilterRegex = GUILayout.TextField(FilterRegex);
            if (GUILayout.Button("Clear", EditorStyles.miniButtonRight,GUILayout.Width(50)))
            {
                GUIUtility.keyboardControl = 0;
                GUIUtility.hotControl = 0;
                FilterRegex = null;
            }
        }
        GUILayout.EndHorizontal();
    }

    public void LogWindow(LogInformation log)
    {
        ///TODO
    }


}
