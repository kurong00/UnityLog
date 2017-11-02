using System;
using UnityEngine;
using System.Text.RegularExpressions;


public class ULogger :MonoBehaviour{

    public static ULogger instance = null;
    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;
    }
    public void Log(string message,int size)
    {
        Debug.Log("<size=" + size + ">" + message + "</size>");
    }

    public void Log(string message, int size,string color)
    {
        Debug.Log("<size=" + size + ">" + "<color=" + color + ">" + message + "</color>" + "</size>");
    }

    public void Log(string message)
    {
        Debug.Log(message);
    }

    public void Log(string message, Color color)
    {
        Debug.Log("<color=" + RGBToHex(color) + ">" + "aaaaaaaaaa" + "</color>");
    }

    public void Log(string message, string color)
    {
        string pattern = @"^[A-Za-z0-9]+$";
        if (color.Length == 6)
        {
            foreach (Match match in Regex.Matches(color, pattern))
                Debug.Log("<color=#" + color + ">" + "aaaaaaaaaa" + "</color>");
        }
        if (color.Length == 7)
        {
            Debug.Log("<color=" + color + ">" + "aaaaaaaaaa" + "</color>");
        }
    }

    public string RGBToHex(Color color)
    {
        int R = Convert.ToInt32(color.r);
        int G = Convert.ToInt32(color.g);
        int B = Convert.ToInt32(color.b);
        string colorR, colorG, colorB;
        if (R == 0)
            colorR = "00";
        if (G == 0)
            colorG = "00";
        if (B == 0)
            colorB = "00";
        colorR = Convert.ToString(R, 16);
        colorG = Convert.ToString(G, 16);
        colorB = Convert.ToString(B, 16);
        string HexColor = "#" + colorR + colorG + colorB;
        return HexColor;
    }

    public Color HexToRGB(string hex)
    {
        Color color = new Color();
        char[] chars = hex.ToCharArray();
        if (chars[0] == '#')
        {
            while (hex.Length == 7)
            {
                color.r = Convert.ToInt32(chars[1]) + Convert.ToInt32(chars[2]);
                color.g = Convert.ToInt32(chars[3]) + Convert.ToInt32(chars[4]);
                color.b = Convert.ToInt32(chars[5]) + Convert.ToInt32(chars[6]);
                return color;
            }
        }
        if (hex.Length == 6)
        {
            color.r = Convert.ToInt32(chars[0]) + Convert.ToInt32(chars[1]);
            color.g = Convert.ToInt32(chars[2]) + Convert.ToInt32(chars[3]);
            color.b = Convert.ToInt32(chars[4]) + Convert.ToInt32(chars[5]);
            return color;
        }
        return color;
    }
}
