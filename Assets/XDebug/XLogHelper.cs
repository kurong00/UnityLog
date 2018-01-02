using UnityEngine;
using System.IO;
using System.Reflection;

static public class XLogGUIConstans
{
    static public readonly string XLOG_EDITOR_NAME = "XLogConsole";
    static public readonly string XLOG_TOOLBAR_BUTTON_CLEAR = "Clear";
    static public readonly string XLOG_TOOLBAR_TOGGLE_ERROR_PAUSE = "Error Pause";
    static public readonly string XLOG_TOOLBAR_TOGGLE_CLEAR_ON_PLAY = "Clear On Play";
    static public readonly string XLOG_TOOLBAR_TOGGLE_SHOW_TIMES = "Show Times";
    static public readonly string XLOG_TOOLBAR_TOGGLE_COLLAPSE = "Collapse";
    static public readonly string XLOG_TOOLBAR_TOGGLE_SCROW_DOWN = "ScrowDown";
    static public readonly string XLOG_TOOLBAR_LABLE_FILTER = "Filter : ";
    static public readonly string XLOG_CHANNEL_ALL = "All";
    static public readonly string XLOG_CHANNEL_DEFAULT = "Default Channel";

    /// <summary>
    ///Use Unity Editor Built-in Icons
    ///http://unitylist.com/r/dba/unity-editor-icons
    /// </summary>
    static public readonly string XLOG_ICON_ERROR = "sv_icon_name6";
    static public readonly string XLOG_ICON_WARNING = "sv_icon_name4";
    static public readonly string XLOG_ICON_MESSAGE = "sv_icon_name0";


    /// <summary>
    /// Use Unity Editor Built-in GUIStyle
    /// https://gist.github.com/MadLittleMods/ea3e7076f0f59a702ecb
    /// </summary>
    static public readonly string XLOG_STYLE_LOG_LINE_ONE = "CN StatusWarn";
    static public readonly string XLOG_STYLE_LOG_LINE_TWO = "CN EntryBackEven";
    static public readonly string XLOG_STYLE_LOG_LINE_THREE = "CN EntryBackOdd";
    static public readonly float XLOG_DOUBLE_CLICK_TIME = 0.3F;
    static public readonly bool XLOG_ADD_UNITY_LOG_SYSTEM = false;
}


public class MarkedLog
{
    public LogInformation Log = null;
    public int Marked = 1;
    public MarkedLog(LogInformation log, int marked = 1)
    {
        Log = log;
        Marked = marked;
    }
}

public class XLogFile : ILogger
{
    private StreamWriter LogFileWriter;
    private bool AddStackFrameInformation;

    public XLogFile(string filename, bool StackFrameInformation = true)
    {
        AddStackFrameInformation = StackFrameInformation;
        var fileLogPath = Path.Combine(Application.dataPath, filename + ".txt");
        LogFileWriter = new StreamWriter(fileLogPath, false);
        LogFileWriter.AutoFlush = true;
    }

    public void Log(LogInformation log)
    {
        lock (this)
        {
            LogFileWriter.WriteLine(log.Message);
            if (AddStackFrameInformation && log.StackFrameList.Count > 0)
            {
                foreach (var frame in log.StackFrameList)
                {
                    LogFileWriter.WriteLine(frame.FormatMethodNameByFile);
                }
                LogFileWriter.WriteLine();
            }
        }
    }
}

class UnityMethod
{
    public enum MethodMode { Show, ShowFirst, Hide }
    public string DeclaringType;
    public string MethodName;
    public MethodMode Mode;

    static UnityMethod[] UnityMethodArray = new UnityMethod[]
    {
        new UnityMethod
        {
            DeclaringType = "Application",
            MethodName = "CallLogCallback",
            Mode = UnityMethod.MethodMode.Hide
        },
        new UnityMethod
        {
            DeclaringType = "DebugLogHandler",
            MethodName = null,
            Mode = UnityMethod.MethodMode.Hide
        },
        new UnityMethod
        {
            DeclaringType = "Logger",
            MethodName = null,
            Mode = UnityMethod.MethodMode.ShowFirst
        },
        new UnityMethod
        {
            DeclaringType = "Debug",
            MethodName = null,
            Mode = UnityMethod.MethodMode.ShowFirst
        },
        new UnityMethod
        {
            DeclaringType = "Assert",
            MethodName = null,
            Mode = UnityMethod.MethodMode.ShowFirst
        }
    };

    public static UnityMethod.MethodMode GetMehodMode(MethodBase method)
    {
        foreach (UnityMethod unityMethod in UnityMethodArray)
        {
            if (unityMethod.DeclaringType == method.DeclaringType.Name &&
                (unityMethod.MethodName == null || method.Name == unityMethod.MethodName))
            {
                return unityMethod.Mode;
            }
        }
        return UnityMethod.MethodMode.Show;
    }
}
