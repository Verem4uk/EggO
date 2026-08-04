using System.Runtime.InteropServices;
using UnityEngine;

public static class WebGLBridge
{
#if UNITY_WEBGL && !UNITY_EDITOR

    [DllImport("__Internal")]
    private static extern void OpenURLInNewTab(string url);

    [DllImport("__Internal")]
    private static extern int GetEggoLevel();

    public static void Open(string url)
    {
        OpenURLInNewTab(url);
    }

    public static int GetLevel()
    {
        return GetEggoLevel();
    }

#else

    public static void Open(string url)
    {
        Debug.Log("WebGLBridge.Open: " + url);
    }

    public static int GetEggoLevel()
    {        
        return 0;
    }

#endif
}