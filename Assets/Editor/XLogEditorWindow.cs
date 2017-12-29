using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;

public class XLogEditorWindow : EditorWindow, ILoggerWindow
{
    [MenuItem("Window/XLogConsole")]
    static void Init()
    {
        XLogEditorWindow xLogEditorWindow = (XLogEditorWindow)
            EditorWindow.GetWindow(typeof(XLogEditorWindow));
        xLogEditorWindow.Show();
    }
    Vector2 LogListScrollPosition;
    Vector2 LogDetailsScrollPosition;
    Texture2D ErrorIcon;
    Texture2D WarningIcon;
    Texture2D MessageIcon;

    bool ShowTimes = true;
    bool Collapse = false;
    bool ScrowDown = false;
    bool Resize = false;
    int SelectedRenderLog = -1;
    bool DirtyLog = false;
    bool MakeDirty = false;
    double LastMessageClickTime = 0;
    double LastFrameClickTime = 0;

    [UnityEngine.SerializeField]
    XLoggerEditor EditorLog;
    List<LogInformation> CurrentLogList = new List<LogInformation>();
    HashSet<string> CurrentChannels = new HashSet<string>();
    Rect CursorChangeRect;
    float CurrentTopPaneHeight;
    Color SizerLineColour;
    GUIStyle EntryStyleBackEven;
    GUIStyle EntryStyleBackOdd;
    string CurrentChannel = null;
    string FilterRegex = null;
    bool ShowErrors = true;
    bool ShowWarnings = true;
    bool ShowMessages = true;
    int SelectedCallstackFrame = 0;
    bool ShowFrameSource = false;
    string SourceLines;
    LogStackFrame SourceLinesFrame;
    void OnEnable()
    {
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
        titleContent.text = XLogGUIConstans.XLOG_EDITOR_NAME;

        ClearSelectedMessage();
        ///Use Unity Editor Built-in Icons
        ///http://unitylist.com/r/dba/unity-editor-icons
        ErrorIcon = EditorGUIUtility.FindTexture(XLogGUIConstans.XLOG_ICON_ERROR);
        WarningIcon = EditorGUIUtility.FindTexture(XLogGUIConstans.XLOG_ICON_WARNING);
        MessageIcon = EditorGUIUtility.FindTexture(XLogGUIConstans.XLOG_ICON_MESSAGE);

        DrawPos = Vector2.zero;
        CurrentTopPaneHeight = position.height / 2;
        DirtyLog = true;
        Repaint();

    }

    Vector2 DrawPos = Vector2.zero;
    public void OnGUI()
    {
        ///
        ///https://gist.github.com/MadLittleMods/ea3e7076f0f59a702ecb
        EntryStyleBackEven = new GUIStyle("CN StatusWarn");
        EntryStyleBackEven.normal = new GUIStyle("CN EntryBackEven").normal;
        EntryStyleBackEven.margin = new RectOffset(5, 10, 5, 10);
        EntryStyleBackEven.border = new RectOffset(5, 0, 5, 0);
        EntryStyleBackEven.fixedHeight = 20;
        EntryStyleBackOdd = new GUIStyle(EntryStyleBackEven);
        EntryStyleBackOdd.normal = new GUIStyle("CN EntryBackOdd").normal;
        if (DirtyLog)
            CurrentLogList = EditorLog.CopyLogInformationList();
        GUILayout.BeginVertical();
        {
            OnGUIToolBar();
            OnGUIFilter();
            OnGUIChannels();
            if (DirtyLog)
            {
                CurrentLogList = EditorLog.CopyLogInformationList();
            }
            float logPanelHeight = CurrentTopPaneHeight + DrawPos.y;
            OnGUILogList(logPanelHeight);
            DrawPos.y += XLogGUIConstans.XLOG_DIVIDER_HEIGHT;
            DrawLogDetails();
        }
        GUILayout.EndVertical();
        DirtyLog = false;
        if (MakeDirty)
        {
            DirtyLog = true;
            MakeDirty = false;
            Repaint();
        }
    }

    void OnGUIToolBar()
    {
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
                    MakeDirty = true;
                    ShowTimes = showTimes;
                    SelectedRenderLog = -1;
                }
                bool collapse = GUILayout.Toggle(Collapse, XLogGUIConstans.XLOG_TOOLBAR_TOGGLE_COLLAPSE,
                    EditorStyles.toolbarButton);
                if (collapse != Collapse)
                {
                    MakeDirty = true;
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
                var showErrors = GUILayout.Toggle(ShowErrors, errorToggleContent, EditorStyles.toolbarButton);
                var showWarnings = GUILayout.Toggle(ShowWarnings, warningToggleContent, EditorStyles.toolbarButton);
                var showMessages = GUILayout.Toggle(ShowMessages, messageToggleContent, EditorStyles.toolbarButton);
                if (showErrors != ShowErrors || showWarnings != ShowWarnings || showMessages != ShowMessages)
                {
                    ClearSelectedMessage();
                    MakeDirty = true;
                }
                ShowWarnings = showWarnings;
                ShowMessages = showMessages;
                ShowErrors = showErrors;
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndHorizontal();
        DrawPos.y = 15;
    }
    void OnGUIFilter()
    {
        string filterRegex = null;
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("Filter : ", GUILayout.Width(40));
            filterRegex = GUILayout.TextField(FilterRegex);
            if (GUILayout.Button("Clear", EditorStyles.miniButtonRight, GUILayout.Width(50)))
            {
                GUIUtility.keyboardControl = 0;
                GUIUtility.hotControl = 0;
                filterRegex = null;
            }
            if (filterRegex != FilterRegex)
            {
                ClearSelectedMessage();
                FilterRegex = filterRegex;
                MakeDirty = true;
            }
        }
        GUILayout.EndHorizontal();
        DrawPos.y += 20;
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
        var drawRect = new Rect(DrawPos, new Vector2(position.width, size.y));
        currentChannelIndex = GUI.SelectionGrid(drawRect, currentChannelIndex, channels.ToArray(), channels.Count);
        if (CurrentChannel != channels[currentChannelIndex])
        {
            CurrentChannel = channels[currentChannelIndex];
            ClearSelectedMessage();
            MakeDirty = true;
        }
        DrawPos.y += size.y;
    }
    void OnGUILogList(float height)
    {
        var oldColor = GUI.backgroundColor;
        float buttonY = 0;
        Regex filterRegex = null;

        if (!string.IsNullOrEmpty(FilterRegex))
        {
            filterRegex = new Regex(FilterRegex);
        }
        var collapseBadgeStyle = EditorStyles.miniButton;
        var logLineStyle = EntryStyleBackEven;
        if (DirtyLog)
        {
            LogListMaxWidth = 0;
            LogListLineHeight = 0;
            CollapseBadgeMaxWidth = 0;
            RenderLogs.Clear();
            if (Collapse)
            {
                var collapsedLines = new Dictionary<string, MarkedLog>();
                var collapsedLinesList = new List<MarkedLog>();
                foreach (var log in CurrentLogList)
                {
                    if (ShouldShowLog(filterRegex, log))
                    {
                        var matchString = log.Message + "!$" + log.LogLevel + "!$" + log.Channel;

                        MarkedLog markedLog;
                        if (collapsedLines.TryGetValue(matchString, out markedLog))
                        {
                            markedLog.Marked++;
                        }
                        else
                        {
                            markedLog = new MarkedLog(log, 1);
                            collapsedLines.Add(matchString, markedLog);
                            collapsedLinesList.Add(markedLog);
                        }
                    }
                }

                foreach (var markedLog in collapsedLinesList)
                {
                    var content = GetLogLineGUIContent(markedLog.Log, ShowTimes);
                    RenderLogs.Add(markedLog);
                    var logLineSize = logLineStyle.CalcSize(content);
                    LogListMaxWidth = Mathf.Max(LogListMaxWidth, logLineSize.x);
                    LogListLineHeight = Mathf.Max(LogListLineHeight, logLineSize.y);

                    var collapseBadgeContent = new GUIContent(markedLog.Marked.ToString());
                    var collapseBadgeSize = collapseBadgeStyle.CalcSize(collapseBadgeContent);
                    CollapseBadgeMaxWidth = Mathf.Max(CollapseBadgeMaxWidth, collapseBadgeSize.x);
                }
            }
            else
            {
                foreach (var log in CurrentLogList)
                {
                    if (ShouldShowLog(filterRegex, log))
                    {
                        var content = GetLogLineGUIContent(log, ShowTimes);
                        RenderLogs.Add(new MarkedLog(log, 1));
                        var logLineSize = logLineStyle.CalcSize(content);
                        LogListMaxWidth = Mathf.Max(LogListMaxWidth, logLineSize.x);
                        LogListLineHeight = Mathf.Max(LogListLineHeight, logLineSize.y);
                    }
                }
            }
            LogListMaxWidth += CollapseBadgeMaxWidth;
        }

        var scrollRect = new Rect(DrawPos, new Vector2(position.width, height));
        float lineWidth = Mathf.Max(LogListMaxWidth, scrollRect.width);

        var contentRect = new Rect(0, 0, lineWidth, RenderLogs.Count * LogListLineHeight);
        Vector2 lastScrollPosition = LogListScrollPosition;
        LogListScrollPosition = GUI.BeginScrollView(scrollRect, LogListScrollPosition, contentRect);
        if (ScrowDown)
        {
            if (lastScrollPosition.y - LogListScrollPosition.y > LogListLineHeight)
            {
                ScrowDown = false;
            }
        }
        float logLineX = CollapseBadgeMaxWidth;
        int firstRenderLogIndex = (int)(LogListScrollPosition.y / LogListLineHeight);
        int lastRenderLogIndex = firstRenderLogIndex + (int)(height / LogListLineHeight);
        firstRenderLogIndex = Mathf.Clamp(firstRenderLogIndex, 0, RenderLogs.Count);
        lastRenderLogIndex = Mathf.Clamp(lastRenderLogIndex, 0, RenderLogs.Count);
        buttonY = firstRenderLogIndex * LogListLineHeight;
        for (int renderLogIndex = firstRenderLogIndex; renderLogIndex < lastRenderLogIndex; renderLogIndex++)
        {
            var markedLog = RenderLogs[renderLogIndex];
            var log = markedLog.Log;

            logLineStyle = (renderLogIndex % 2 == 0) ? EntryStyleBackEven : EntryStyleBackOdd;

            if (renderLogIndex == SelectedRenderLog)
            {
                GUI.backgroundColor = Color.yellow;
            }
            else
            {
                GUI.backgroundColor = Color.white;
            }
            var content = GetLogLineGUIContent(log, ShowTimes);
            var drawRect = new Rect(logLineX, buttonY, contentRect.width, LogListLineHeight);
            if (GUI.Button(drawRect, content, logLineStyle))
            {
                if (renderLogIndex == SelectedRenderLog)
                {
                    if (EditorApplication.timeSinceStartup - LastMessageClickTime <
                        XLogGUIConstans.XLOG_DOUBLE_CLICK_TIME)
                    {
                        LastMessageClickTime = 0;
                        for (int frame = 0; frame < log.StackFrameList.Count; frame++)
                        {
                            if (JumpToSource(log.StackFrameList[frame]))
                                break;
                        }
                    }
                    else
                    {
                        LastMessageClickTime = EditorApplication.timeSinceStartup;
                    }
                }
                else
                {
                    SelectedRenderLog = renderLogIndex;
                    SelectedCallstackFrame = -1;
                    LastMessageClickTime = EditorApplication.timeSinceStartup;
                }
                var go = log.OriginObject as GameObject;
                if (go != null)
                {
                    Selection.activeGameObject = go;
                }
            }

            if (Collapse)
            {
                var collapseBadgeContent = new GUIContent(markedLog.Marked.ToString());
                var collapseBadgeSize = collapseBadgeStyle.CalcSize(collapseBadgeContent);
                var collapseBadgeRect = new Rect(0, buttonY, collapseBadgeSize.x, collapseBadgeSize.y);
                GUI.Button(collapseBadgeRect, collapseBadgeContent, collapseBadgeStyle);
            }
            buttonY += LogListLineHeight;
        }
        if (ScrowDown && RenderLogs.Count > 0)
        {
            LogListScrollPosition.y = ((RenderLogs.Count + 1) * LogListLineHeight) - scrollRect.height;
        }

        GUI.EndScrollView();
        DrawPos.y += height;
        DrawPos.x = 0;
        GUI.backgroundColor = oldColor;
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

    GUIContent GetLogLineGUIContent(LogInformation log, bool showTimes)
    {
        var showMessage = log.Message;
        showMessage = showMessage.Replace(XLogger.DirectorySeparator, ' ');
        if (showTimes)
        {
            showMessage = log.RelativeTimeLine + ": " + showMessage;
        }
        var content = new GUIContent(showMessage, GetIconForLog(log));
        return content;
    }


    public void DrawLogDetails()
    {
        var oldColor = GUI.backgroundColor;
        SelectedRenderLog = Mathf.Clamp(SelectedRenderLog, 0, CurrentLogList.Count);
        if (RenderLogs.Count > 0 && SelectedRenderLog >= 0)
        {
            var selectedLog = RenderLogs[SelectedRenderLog];
            var log = selectedLog.Log;
            var logLineStyle = EntryStyleBackEven;
            var sourceStyle = new GUIStyle(GUI.skin.textArea);
            sourceStyle.richText = true;
            var drawRect = new Rect(DrawPos, new Vector2(position.width - DrawPos.x, position.height - DrawPos.y));
            var detailLines = new List<GUIContent>();
            float contentHeight = 0;
            float contentWidth = 0;
            float lineHeight = 0;
            for (int i = 0; i < log.StackFrameList.Count; i++)
            {
                var frame = log.StackFrameList[i];
                var methodName = frame.FormatMethodNameByFile;
                if (!string.IsNullOrEmpty(methodName))
                {
                    var content = new GUIContent(methodName);
                    detailLines.Add(content);

                    var contentSize = logLineStyle.CalcSize(content);
                    contentHeight += contentSize.y;
                    lineHeight = Mathf.Max(lineHeight, contentSize.y);
                    contentWidth = Mathf.Max(contentSize.x, contentWidth);
                    if (ShowFrameSource && i == SelectedCallstackFrame)
                    {
                        var sourceContent = GetFrameSourceGUIContent(frame);
                        if (sourceContent != null)
                        {
                            var sourceSize = sourceStyle.CalcSize(sourceContent);
                            contentHeight += sourceSize.y;
                            contentWidth = Mathf.Max(sourceSize.x, contentWidth);
                        }
                    }
                }
            }
            var contentRect = new Rect(0, 0, Mathf.Max(contentWidth, drawRect.width), contentHeight);

            LogDetailsScrollPosition = GUI.BeginScrollView(drawRect, LogDetailsScrollPosition, contentRect);

            float lineY = 0;
            for (int i = 0; i < detailLines.Count; i++)
            {
                var lineContent = detailLines[i];
                if (lineContent != null)
                {
                    logLineStyle = (i % 2 == 0) ? EntryStyleBackEven : EntryStyleBackOdd;
                    if (i == SelectedCallstackFrame)
                    {
                        GUI.backgroundColor = Color.gray;
                    }
                    else
                    {
                        GUI.backgroundColor = Color.white;
                    }
                    var frame = log.StackFrameList[i];
                    var lineRect = new Rect(0, lineY, contentRect.width, lineHeight);
                    if (GUI.Button(lineRect, lineContent, logLineStyle))
                    {
                        if (i == SelectedCallstackFrame)
                        {
                            if (Event.current.button == 1)
                            {
                                ToggleShowSource(frame);
                                Repaint();
                            }
                            else
                            {
                                if (EditorApplication.timeSinceStartup - LastFrameClickTime <
                                    XLogGUIConstans.XLOG_DOUBLE_CLICK_TIME)
                                {
                                    LastFrameClickTime = 0;
                                    JumpToSource(frame);
                                }
                                else
                                {
                                    LastFrameClickTime = EditorApplication.timeSinceStartup;
                                }
                            }
                        }
                        else
                        {
                            SelectedCallstackFrame = i;
                            LastFrameClickTime = EditorApplication.timeSinceStartup;
                        }
                    }
                    lineY += lineHeight;
                    if (ShowFrameSource && i == SelectedCallstackFrame)
                    {
                        GUI.backgroundColor = Color.white;

                        var sourceContent = GetFrameSourceGUIContent(frame);
                        if (sourceContent != null)
                        {
                            var sourceSize = sourceStyle.CalcSize(sourceContent);
                            var sourceRect = new Rect(0, lineY, contentRect.width, sourceSize.y);

                            GUI.Label(sourceRect, sourceContent, sourceStyle);
                            lineY += sourceSize.y;
                        }
                    }
                }
            }
            GUI.EndScrollView();
        }
        GUI.backgroundColor = oldColor;
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

    void ToggleShowSource(LogStackFrame frame)
    {
        ShowFrameSource = !ShowFrameSource;
    }

    bool JumpToSource(LogStackFrame frame)
    {
        if (frame.FileName != null)
        {
            var osFileName = XLogger.ConvertDirectorySeparatorsFromUnityToOS(frame.FileName);
            var filename = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), osFileName);
            if (System.IO.File.Exists(filename))
            {
                if (UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal(filename, frame.LineNumber))
                    return true;
            }
        }

        return false;
    }

    GUIContent GetFrameSourceGUIContent(LogStackFrame frame)
    {
        var source = GetSourceForFrame(frame);
        if (!string.IsNullOrEmpty(source))
        {
            var content = new GUIContent(source);
            return content;
        }
        return null;
    }

    List<string> GetChannels()
    {
        if (DirtyLog)
        {
            CurrentChannels = EditorLog.CopyChannels();
        }
        var categories = CurrentChannels;
        var channelList = new List<string>();
        channelList.Add(XLogGUIConstans.XLOG_CHANNEL_ALL);
        channelList.Add(XLogGUIConstans.XLOG_CHANNEL_DEFAULT);
        channelList.AddRange(categories);
        return channelList;
    }

    string GetSourceForFrame(LogStackFrame frame)
    {
        if (SourceLinesFrame == frame)
        {
            return SourceLines;
        }

        if (frame.FileName == null)
        {
            return "";
        }

        var osFileName = XLogger.ConvertDirectorySeparatorsFromUnityToOS(frame.FileName);
        var filename = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), osFileName);
        if (!System.IO.File.Exists(filename))
        {
            return "";
        }

        int lineNumber = frame.LineNumber - 1;
        int linesAround = 3;
        var lines = System.IO.File.ReadAllLines(filename);
        var firstLine = Mathf.Max(lineNumber - linesAround, 0);
        var lastLine = Mathf.Min(lineNumber + linesAround + 1, lines.Length);

        SourceLines = "";
        if (firstLine != 0)
        {
            SourceLines += "...\n";
        }
        for (int i = firstLine; i < lastLine; i++)
        {
            string str = lines[i] + "\n";
            SourceLines += str;
        }
        if (lastLine != lines.Length)
        {
            SourceLines += "...\n";
        }

        SourceLinesFrame = frame;
        return SourceLines;
    }

    void ClearSelectedMessage()
    {
        SelectedRenderLog = -1;
        SelectedCallstackFrame = -1;
        ShowFrameSource = false;
    }

    public void LogWindow(LogInformation log)
    {
        DirtyLog = true;
    }

    List<MarkedLog> RenderLogs = new List<MarkedLog>();
    float LogListMaxWidth = 0;
    float LogListLineHeight = 0;
    float CollapseBadgeMaxWidth = 0;

}
