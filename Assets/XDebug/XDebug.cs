
public static class XDebug {
    [ExcludeStackTrace]
    static public void Log(UnityEngine.Object context, string message, params object[] paramsObject)
    {
        XLogger.Log(context,LogLevel.Message,"", message, paramsObject);
    }
    [ExcludeStackTrace]
    static public void Log(string message, params object[] paramsObject)
    {
        XLogger.Log(null,LogLevel.Message, "", message, paramsObject);
    }
}
