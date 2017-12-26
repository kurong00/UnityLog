using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

class UnityMethod
{
    public enum MethodMode { Show,ShowFirst,Hide }
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
        foreach(UnityMethod unityMethod in UnityMethodArray)
        {
            if(unityMethod.DeclaringType == method.DeclaringType.Name &&
                (unityMethod.MethodName == null || method.Name == unityMethod.MethodName))
            {
                return unityMethod.Mode;
            }
        }
        return UnityMethod.MethodMode.Show;
    }
}



