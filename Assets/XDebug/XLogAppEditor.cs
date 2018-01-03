using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

class XLogAppEditor : MonoBehaviour, ILogger
{
    public int FontSize = 0;

    List<LogInformation> LogList = new List<LogInformation>();
    int Errors = 0;
    int Warnings = 0;
    int Messages = 0;
    int SelectedMessage = -1;
    int SelectedCallstackFrame = 0;
    bool PauseOnError = false;
    bool ShowTimes = false;
    Rect WindowRect;
    float TopHeight;
    bool ShowWindow = true;

    GUIStyle LogLineStyle1;
    GUIStyle LogLineStyle2;
    GUIStyle SelectedLogLineStyle;
    Texture2D ErrorIcon;
    Texture2D WarningIcon;
    Texture2D MessageIcon;
    bool ShowErrors = true;
    bool ShowWarnings = true;
    bool ShowMessages = true;
    string CurrentChannel = null;
    string FilterRegex = "";

    void Start()
    {
        DontDestroyOnLoad(gameObject);
        XLogger.AddLogger(this);
        WindowRect = new Rect(0, 0, Screen.width / 2, Screen.height);
        TopHeight = Screen.height * 0.75f;
        ClearSelectedMessage();
        ErrorIcon = EditorGUIUtility.FindTexture(XLogGUIConstans.XLOG_ICON_ERROR);
        WarningIcon = EditorGUIUtility.FindTexture(XLogGUIConstans.XLOG_ICON_WARNING);
        MessageIcon = EditorGUIUtility.FindTexture(XLogGUIConstans.XLOG_ICON_MESSAGE);
    }

    public void OnGUI()
    {
        if (ShowWindow)
        {
            GUI.color = new Color(1, 1, 1, 0.5f);
            /*LogLineStyle1 = new GUIStyle("CN StatusWarn");
            LogLineStyle2 = new GUIStyle("CN StatusWarn");
            SelectedLogLineStyle = new GUIStyle(XLogGUIConstans.XLOG_STYLE_LOG_LINE_THREE);
            LogLineStyle1.fontSize = FontSize;
            LogLineStyle2.fontSize = FontSize;
            SelectedLogLineStyle.fontSize = FontSize;*/
            WindowRect = GUILayout.Window(1, WindowRect, DrawWindow, XLogGUIConstans.XLOG_EDITOR_NAME, GUI.skin.window);
        }
    }

    void DrawWindow(int windowID)
    {
        GUILayout.BeginVertical();
        {
            OnGUIToolBar();
            OnGUIFilter();
            OnGUIChannels();
            //OnGUILogList(TopHeight);
        }
        GUILayout.EndVertical();
    }

    void OnGUIToolBar()
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button(XLogGUIConstans.XLOG_TOOLBAR_BUTTON_CLEAR, EditorStyles.toolbarButton))
                    Clear();
                ShowTimes = GUILayout.Toggle(ShowTimes, XLogGUIConstans.XLOG_TOOLBAR_TOGGLE_SHOW_TIMES,
                    EditorStyles.toolbarButton);
            }
            GUILayout.EndHorizontal();
            GUIContent errorToggleContent = new GUIContent(Errors.ToString(), ErrorIcon);
            GUIContent warningToggleContent = new GUIContent(Warnings.ToString(), WarningIcon);
            GUIContent messageToggleContent = new GUIContent(Messages.ToString(), MessageIcon);
            float totalErrorButtonWidth = EditorStyles.toolbarButton.CalcSize(errorToggleContent).x
                + EditorStyles.toolbarButton.CalcSize(warningToggleContent).x +
                EditorStyles.toolbarButton.CalcSize(messageToggleContent).x;

            GUILayout.Space(20);

            GUILayout.BeginHorizontal();
            {
                var showErrors = GUILayout.Toggle(ShowErrors, errorToggleContent, EditorStyles.toolbarButton);
                var showWarnings = GUILayout.Toggle(ShowWarnings, warningToggleContent, EditorStyles.toolbarButton);
                var showMessages = GUILayout.Toggle(ShowMessages, messageToggleContent, EditorStyles.toolbarButton);
                if (showErrors != ShowErrors || showWarnings != ShowWarnings || showMessages != ShowMessages)
                {
                    ClearSelectedMessage();
                }
                ShowWarnings = showWarnings;
                ShowMessages = showMessages;
                ShowErrors = showErrors;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndHorizontal();
    }
    void OnGUIFilter()
    {
        string filterRegex = null;
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label(XLogGUIConstans.XLOG_TOOLBAR_LABLE_FILTER, GUILayout.Width(40));
            filterRegex = GUILayout.TextField(FilterRegex);
            if (GUILayout.Button(XLogGUIConstans.XLOG_TOOLBAR_BUTTON_CLEAR,
                EditorStyles.miniButtonRight, GUILayout.Width(50)))
            {
                GUIUtility.keyboardControl = 0;
                GUIUtility.hotControl = 0;
                filterRegex = null;
            }
            if (filterRegex != FilterRegex)
            {
                ClearSelectedMessage();
                FilterRegex = filterRegex;
            }
        }
        GUILayout.EndHorizontal();
    }
    void OnGUIChannels()
    {
        var channels = GetChannels();
        int currentChannelIndex = 0;
        for (int i = 0; i < channels.Count; i++)
        {
            if (channels[i] == CurrentChannel)
            {
                currentChannelIndex = i;
                break;
            }
        }
        var content = new GUIContent(" ");
        var size = GUI.skin.button.CalcSize(content);
        currentChannelIndex = GUILayout.SelectionGrid(currentChannelIndex, channels.ToArray(), channels.Count);
        if (CurrentChannel != channels[currentChannelIndex])
        {
            CurrentChannel = channels[currentChannelIndex];
            ClearSelectedMessage();
        }
    }

    List<string> GetChannels()
    {
        var categories = new HashSet<string>();
        foreach(LogInformation log in LogList)
        {
            if (!string.IsNullOrEmpty(log.Channel) && !categories.Contains(log.Channel))
                categories.Add(log.Channel);
        }
        var channelList = new List<string>();
        channelList.Add(XLogGUIConstans.XLOG_CHANNEL_ALL);
        channelList.Add(XLogGUIConstans.XLOG_CHANNEL_DEFAULT);
        channelList.AddRange(categories);
        return channelList;
    }

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

    void ClearSelectedMessage()
    {
        SelectedCallstackFrame = -1;
        SelectedMessage = -1;
    }
    
}
