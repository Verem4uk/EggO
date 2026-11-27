using System.Runtime.InteropServices;
using UnityEngine;

public static class WebGLBridge
{
#if UNITY_WEBGL && !UNITY_EDITOR

    [DllImport("__Internal")]
    private static extern void OpenURLInNewTab(string url);

    [DllImport("__Internal")]
    private static extern int GetEggoLevel();

    [DllImport("__Internal")]
    private static extern int GetEggoTrial();

    [DllImport("__Internal")]
    private static extern void RegisterEggoAccountReceiver(string objectName);

    [DllImport("__Internal")]
    private static extern int IsEggoAccountReady();

    [DllImport("__Internal")]
    private static extern int IsEggoSignedIn();

    [DllImport("__Internal")]
    private static extern int CanPlayEggoLevel(int level);

    [DllImport("__Internal")]
    private static extern void RequestEggoRegistration();

    [DllImport("__Internal")]
    private static extern void SaveEggoLevel(int level);

    [DllImport("__Internal")]
    private static extern void SetEggoAccountLanguage(int index);

    public static void RegisterAccountReceiver(string objectName) => RegisterEggoAccountReceiver(objectName);
    public static bool IsAccountReady() => IsEggoAccountReady() == 1;
    public static bool IsSignedIn() => IsEggoSignedIn() == 1;
    public static bool CanPlayLevel(int level) => CanPlayEggoLevel(level) == 1;
    public static void RequestRegistration() => RequestEggoRegistration();
    public static void SaveLevel(int level) => SaveEggoLevel(level);
    public static void SetAccountLanguage(int index) => SetEggoAccountLanguage(index);

    public static void Open(string url)
    {
        OpenURLInNewTab(url);
    }

    public static int GetLevel()
    {
        return GetEggoLevel();
    }

    public static bool IsTrial()
    {
        return GetEggoTrial() == 1;
    }

#else

    public static void Open(string url)
    {
        Debug.Log("WebGLBridge.Open: " + url);
    }

    public static int GetLevel()
    {        
        return 0;
    }

    public static bool IsTrial()
    {
        return false;
    }

    public static void RegisterAccountReceiver(string objectName) { }
    public static bool IsAccountReady() => true;
    public static bool IsSignedIn() => true;
    public static bool CanPlayLevel(int level) => level <= Saver.MaxAvailableLevel();
    public static void RequestRegistration() { }
    public static void SaveLevel(int level) { }
    public static void SetAccountLanguage(int index) { }

#endif
}
