using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.Text.RegularExpressions;

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
    bool ShowTimes;
    bool Collapse;
    bool DirtyLog;
    bool ScrowDown;
    bool ShowErrors;
    bool ShowWarnings;
    bool ShowMessage;
    string FilterRegex;
    string CurrentChannel;
    float LogListMaxWidth = 0;
    float LogListLineHeight = 0;
    float CollapseBadgeMaxWidth = 0;
    List<LogInformation> CurrentLogList = new List<LogInformation>();
    List<MarkedLog> RenderLogs = new List<MarkedLog>();

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
            OnGUILogList();
        }
        GUILayout.EndVertical();
        if (DirtyLog)
            CurrentLogList = EditorLog.CopyLogInformationList();
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
                ShowErrors = GUILayout.Toggle(ShowErrors, errorToggleContent, EditorStyles.toolbarButton);
                ShowWarnings = GUILayout.Toggle(ShowWarnings, warningToggleContent, EditorStyles.toolbarButton);
                ShowMessage = GUILayout.Toggle(ShowMessage, messageToggleContent, EditorStyles.toolbarButton);
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndHorizontal();
    }

    void OnGUIFilter()
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Filter : ", GUILayout.Width(40));
            FilterRegex = GUILayout.TextField(FilterRegex);
            if (GUILayout.Button("Clear", EditorStyles.miniButtonRight, GUILayout.Width(50)))
            {
                GUIUtility.keyboardControl = 0;
                GUIUtility.hotControl = 0;
                FilterRegex = null;
            }
        }
        GUILayout.EndHorizontal();
    }

    void OnGUILogList()
    {
        Regex regex = null;
        if (!string.IsNullOrEmpty(FilterRegex))
            regex = new Regex(FilterRegex);
        if (DirtyLog)
        {
            LogListMaxWidth = 0;
            LogListLineHeight = 0;
            CollapseBadgeMaxWidth = 0;
            RenderLogs.Clear();
        }
        if (Collapse)
        {
            var collapsedLinesDictionary = new Dictionary<string, MarkedLog>();
            var collapsedLinesList = new List<MarkedLog>();

            foreach (var log in CurrentLogList)
            {
                if (IsShowingLog(regex, log))
                {
                    var matchString = log.Message + "!$" + log.LogLevel + "!$" + log.Channel;

                    MarkedLog markedLog;
                    if (collapsedLinesDictionary.TryGetValue(matchString, out markedLog))
                    {
                        markedLog.Marked++;
                    }
                    else
                    {
                        markedLog = new MarkedLog(log);
                        collapsedLinesDictionary.Add(matchString, markedLog);
                        collapsedLinesList.Add(markedLog);
                    }
                }
            }

            foreach (var markedLog in collapsedLinesList)
            {
                var content = GetLogLineGUIContent(markedLog.Log, ShowTimes);
                RenderLogs.Add(markedLog);
                var logLineSize = LogStyle.CalcSize(content);
                LogListMaxWidth = Mathf.Max(LogListMaxWidth, logLineSize.x);
                LogListLineHeight = Mathf.Max(LogListLineHeight, logLineSize.y);
                var collapseBadgeContent = new GUIContent(markedLog.Marked.ToString());
                var collapseBadgeSize = EditorStyles.miniButton.CalcSize(collapseBadgeContent);
                CollapseBadgeMaxWidth = Mathf.Max(CollapseBadgeMaxWidth, collapseBadgeSize.x);
            }
        }
        else
        {
            foreach (var log in CurrentLogList)
            {
                if (IsShowingLog(regex, log))
                {
                    var content = GetLogLineGUIContent(log, ShowTimes);
                    RenderLogs.Add(new MarkedLog(log));
                    var logLineSize = LogStyle.CalcSize(content);
                    LogListMaxWidth = Mathf.Max(LogListMaxWidth, logLineSize.x);
                    LogListLineHeight = Mathf.Max(LogListLineHeight, logLineSize.y);
                }
            }
        }
        var scrollPosition = Vector2.zero;
        GUILayout.BeginScrollView(scrollPosition,GUILayout.Width(position.width),
            GUILayout.Height(position.height-50));
        {
            string s = " ";
            for(int i = 0; i < 100; i++)
            {
                s += "aaaaaaaaa";
            }
            for (int i = 0; i < 20; i++)
            {
                EditorGUILayout.LabelField(s);
            }
        }
        GUILayout.EndScrollView();
        LogListMaxWidth += CollapseBadgeMaxWidth;
    }

    GUIContent GetLogLineGUIContent(LogInformation log, bool showTimes)
    {
        var showMessage = log.Message;
        showMessage = showMessage.Replace(XLogger.DirectorySeparator, " ");
        if (showTimes)
        {
            showMessage = string.Format("{0:0.0000}", log.RelativeTimeLine) + ": " + showMessage;
        }

        var content = new GUIContent(showMessage, GetIconForLog(log));
        return content;
    }

    Texture2D GetIconForLog(LogInformation log)
    {
        if (log.LogLevel == LogLevel.Error)
        {
            return ErrorIcon;
        }
        if (log.LogLevel == LogLevel.Warning)
        {
            return WarningIcon;
        }

        return MessageIcon;
    }

    public void LogWindow(LogInformation log)
    {
        ///TODO
    }

    bool IsShowingLog(Regex regex, LogInformation log)
    {
        if (log.Channel == CurrentChannel || CurrentChannel == "All" || (CurrentChannel == "No Channel"
            && string.IsNullOrEmpty(log.Channel)))
        {
            if ((log.LogLevel == LogLevel.Error && ShowErrors)
               || (log.LogLevel == LogLevel.Warning && ShowWarnings)
               || (log.LogLevel == LogLevel.Message && ShowMessage))
            {
                if (regex == null || regex.IsMatch(log.Message))
                {
                    return true;
                }
            }
        }
        return false;
    }

}
