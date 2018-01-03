using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

class XLogAppEditor : MonoBehaviour, ILogger
{
    public int FontSize = 0;
    public GUISkin Skin;

    List<LogInformation> LogList = new List<LogInformation>();
    int Errors = 0;
    int Warnings = 0;
    int Messages = 0;
    int SelectedRenderLog = -1;
    int SelectedCallstackFrame = 0;
    double LastMessageClickTime = 0;
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

    Vector2 LogListScrollPosition;
    Vector2 LogDetailsScrollPosition;

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
            LogLineStyle1 = Skin.customStyles[0];
            LogLineStyle2 = Skin.customStyles[1];
            SelectedLogLineStyle = Skin.customStyles[2];
            LogLineStyle1.fontSize = FontSize;
            LogLineStyle2.fontSize = FontSize;
            SelectedLogLineStyle.fontSize = FontSize;/**/
            WindowRect = GUILayout.Window(1, WindowRect, DrawWindow, 
                XLogGUIConstans.XLOG_EDITOR_NAME, Skin.window);
        }
    }

    void DrawWindow(int windowID)
    {
        GUILayout.BeginVertical();
        {
            OnGUIToolBar();
            OnGUIFilter();
            OnGUIChannels();
            OnGUILogList();
            OnGUILogDetails();
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

    void OnGUILogList()
    {
        var oldColor = GUI.backgroundColor;
        SelectedRenderLog = Mathf.Clamp(SelectedRenderLog, 0, LogList.Count);
        if (LogList.Count > 0 && SelectedRenderLog >= 0)
        {
            LogListScrollPosition = GUILayout.BeginScrollView(LogListScrollPosition);
            var maxLogPanelHeight = WindowRect.height;
            float buttonY = 0;
            float buttonHeight = LogLineStyle1.CalcSize(new GUIContent("Test")).y;
            Regex filterRegex = null;
            if (!String.IsNullOrEmpty(FilterRegex))
            {
                filterRegex = new Regex(FilterRegex);
            }
            int drawnButtons = 0;
            GUIStyle logLineStyle = LogLineStyle1;
            for (int i = 0; i < LogList.Count; i++)
            {
                LogInformation log = LogList[i];
                if (ShouldShowLog(filterRegex, log))
                {
                    drawnButtons++;
                    if (buttonY + buttonHeight > LogListScrollPosition.y && buttonY < LogListScrollPosition.y + maxLogPanelHeight)
                    {
                        if (i == SelectedRenderLog)
                        {
                            logLineStyle = SelectedLogLineStyle;
                        }
                        else
                        {
                            logLineStyle = (drawnButtons % 2 == 0) ? LogLineStyle1 : LogLineStyle2;
                        }

                        var showMessage = log.Message;

                        showMessage = showMessage.Replace(XLogger.DirectorySeparator, ' ');
                        if (ShowTimes)
                        {
                            showMessage = log.RelativeTimeLine + ": " + showMessage;
                        }

                        var content = new GUIContent(showMessage, GetIconForLog(log));
                        if (GUILayout.Button(content, logLineStyle, GUILayout.Height(buttonHeight)))
                        {
                            if (i == SelectedRenderLog)
                            {
                                if (Time.realtimeSinceStartup - LastMessageClickTime < 0.3f)
                                {
                                    LastMessageClickTime = 0;
                                }
                                else
                                {
                                    LastMessageClickTime = Time.realtimeSinceStartup;
                                }
                            }
                            else
                            {
                                SelectedRenderLog = i;
                                SelectedCallstackFrame = -1;
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Space(buttonHeight);
                    }
                    buttonY += buttonHeight;
                }
            }
            GUILayout.EndScrollView();
        }
        GUI.backgroundColor = oldColor;
    }
    List<string> GetChannels()
    {
        var categories = new HashSet<string>();
        foreach (LogInformation log in LogList)
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

    bool ShouldShowLog(Regex regex, LogInformation log)
    {
        if (log.Channel == CurrentChannel || CurrentChannel == XLogGUIConstans.XLOG_CHANNEL_ALL ||
            (CurrentChannel == XLogGUIConstans.XLOG_CHANNEL_DEFAULT && string.IsNullOrEmpty(log.Channel)))
        {
            if ((log.LogLevel == LogLevel.Message && ShowMessages)
               || (log.LogLevel == LogLevel.Warning && ShowWarnings)
               || (log.LogLevel == LogLevel.Error && ShowErrors))
            {
                if (regex == null || regex.IsMatch(log.Message))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public void OnGUILogDetails()
    {
        var oldColor = GUI.backgroundColor;
        SelectedRenderLog = Mathf.Clamp(SelectedRenderLog, 0, LogList.Count);
        if (LogList.Count > 0 && SelectedRenderLog >= 0)
        {
            LogDetailsScrollPosition = GUILayout.BeginScrollView(LogDetailsScrollPosition);
            var log = LogList[SelectedRenderLog];
            var logLineStyle = LogLineStyle1;
            for (int i = 0; i < log.StackFrameList.Count; i++)
            {
                var frame = log.StackFrameList[i];
                var methodName = frame.FormatMethodNameByFile;
                if (!String.IsNullOrEmpty(methodName))
                {
                    if (i == SelectedCallstackFrame)
                    {
                        logLineStyle = SelectedLogLineStyle;
                    }
                    else
                    {
                        logLineStyle = (i % 2 == 0) ? LogLineStyle1 : LogLineStyle2;
                    }
                    if (GUILayout.Button(methodName, logLineStyle))
                    {
                        SelectedCallstackFrame = i;
                    }
                }

            }
            GUILayout.EndScrollView();
        }
        GUI.backgroundColor = oldColor;
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
        SelectedRenderLog = -1;
    }

}
