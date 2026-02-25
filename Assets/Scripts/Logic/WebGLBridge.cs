using System.Runtime.InteropServices;
using UnityEngine;

public static class WebGLBridge
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void OpenURLInNewTab(string url);

    public static void Open(string url)
    {
        OpenURLInNewTab(url);
    }
#else
    public static void Open(string url)
    {
        Debug.Log("WebGLBridge.Open: " + url);
    }
#endif
}