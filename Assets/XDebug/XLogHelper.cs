using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

static public class XLogGUIConstans
{
    static public readonly int XLOG_DETAIL_LINE_HEIGHT = 20;
    static public readonly string XLOG_TOOLBAR_BUTTON_CLEAR = "Clear";
}

static public class XLogGUIFunc
{
    static public bool AdaptingButton(string text, GUIStyle style, Vector2 drawPos, out Vector2 size)
    {
        var content = new GUIContent(text);
        size = style.CalcSize(content);
        var rect = new Rect(drawPos, size);
        return GUI.Button(rect, text, style);
    }

    static public bool AdaptingToggle(bool state, GUIContent content, GUIStyle style, Vector2 drawPos, out Vector2 size)
    {
        size = style.CalcSize(content);
        Rect drawRect = new Rect(drawPos, size);
        return GUI.Toggle(drawRect, state, content, style);
    }

    static public bool AdaptingToggle(bool state, string text, GUIStyle style, Vector2 drawPos, out Vector2 size)
    {
        var content = new GUIContent(text);
        return AdaptingToggle(state, content,style,drawPos,out size);
    }

    static public void AdaptingLable(string text, GUIStyle style,Vector2 drawPos ,out Vector2 size)
    {
        var content = new GUIContent(text);
        size = style.CalcSize(content);
        Rect drawRect = new Rect(drawPos, size);
        GUI.Label(drawRect, text, style);
    }
}
