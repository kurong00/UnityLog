public static class XDebug
{

    //MessageLog
    [ExcludeStackTrace]
    static public void Log(UnityEngine.Object context, string message, params object[] paramsObject)
    {
        XLogger.Log(context, LogLevel.Message, "", message, paramsObject);
    }
    [ExcludeStackTrace]
    static public void Log(string message, params object[] paramsObject)
    {
        XLogger.Log(null, LogLevel.Message, "", message, paramsObject);
    }

    //WarningLog
    [ExcludeStackTrace]
    static public void LogWarning(UnityEngine.Object context, string message, params object[] paramsObject)
    {
        XLogger.Log(context, LogLevel.Warning, "", message, paramsObject);
    }
    [ExcludeStackTrace]
    static public void LogWarning(string message, params object[] paramsObject)
    {
        XLogger.Log(null, LogLevel.Warning, "", message, paramsObject);
    }

    //ErrorLog
    [ExcludeStackTrace]
    static public void LogError(UnityEngine.Object context, string message, params object[] paramsObject)
    {
        XLogger.Log(context, LogLevel.Error, "", message, paramsObject);
    }
    [ExcludeStackTrace]
    static public void LogError(string message, params object[] paramsObject)
    {
        XLogger.Log(null, LogLevel.Error, "", message, paramsObject);
    }

    [ExcludeStackTrace]
    static public void LogChannel(UnityEngine.Object context, string channel, string message, params object[] paramsObject)
    {
        XLogger.Log(context, LogLevel.Message, channel, message, paramsObject);
    }
    [ExcludeStackTrace]
    static public void LogChannel(string channel, string message, params object[] paramsObject)
    {
        XLogger.Log(null, LogLevel.Message, channel, message, paramsObject);
    }


    [ExcludeStackTrace]
    static public void LogWarningChannel(UnityEngine.Object context, string channel, string message, params object[] paramsObject)
    {
        XLogger.Log(context, LogLevel.Warning, channel, message, paramsObject);
    }
    [ExcludeStackTrace]
    static public void LogWarningChannel(string channel, string message, params object[] paramsObject)
    {
        XLogger.Log(null, LogLevel.Warning, channel, message, paramsObject);
    }

    [ExcludeStackTrace]
    static public void LogErrorChannel(UnityEngine.Object context, string channel, string message, params object[] paramsObject)
    {
        XLogger.Log(context, LogLevel.Error, channel, message, paramsObject);
    }
    [ExcludeStackTrace]
    static public void LogErrorChannel(string channel, string message, params object[] paramsObject)
    {
        XLogger.Log(null, LogLevel.Error, channel, message, paramsObject);
    }
}
