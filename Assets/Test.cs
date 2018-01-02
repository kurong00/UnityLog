using UnityEngine;
public class Test : MonoBehaviour
{
    void Start()
    {
        XLogger.AddLogger(new XLogFile("MyLog"));
        XDebug.Log("XLoger Message");
        XDebug.Log("XLoger Message");
        XDebug.Log("XLoger Message");
        XDebug.Log("XLoger Message");
        XDebug.Log("XLoger Message");
        XDebug.Log("XLoger Message");
        XDebug.LogWarning("XLoger Warning");
        XDebug.LogError("XLoger Error");

        XDebug.LogChannel("Game", "I am a message fron channel: Game");
        XDebug.LogChannel("Data", "I am a message fron channel: Data");
        XDebug.LogChannel("Time", "I am a message fron channel: Time");

        XDebug.LogWarningChannel("Game", "I am a message fron channel: Game");
        XDebug.LogWarningChannel("Data", "I am a message fron channel: Data");
        XDebug.LogWarningChannel("Time", "I am a message fron channel: Time");

        XDebug.LogErrorChannel("Game", "I am a message fron channel: Game");
        XDebug.LogErrorChannel("Data", "I am a message fron channel: Data");
        XDebug.LogErrorChannel("Time", "I am a message fron channel: Time");
    }
}
