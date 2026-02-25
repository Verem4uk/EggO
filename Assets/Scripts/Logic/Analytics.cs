using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public static class Analytics
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void SendAnalyticsEvent(string eventName, int level, float duration);
#endif

    private static void LogEvent(string eventName, int level = 0, float duration = 0f)
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SendAnalyticsEvent(eventName, level, duration);
#else
        Debug.Log($"[Analytics] {eventName} | level: {level} | duration: {duration}");
#endif
    }

    public static void StartSession(int level)
    {
        Debug.Log("Session started " + level);
        LogEvent("session_start", level);
    }

    public static void FinishSession(int level, int duration)
    {
        Debug.Log("Session finished " + level + " " + duration);
        LogEvent("session_complete", level, duration);
    }

    public static void StartPurchase(int level)
    {
        Debug.Log("Start purchase " + level);
        LogEvent("purchase_start", level);
    }

    public static void RefusePurchase(int level)
    {
        Debug.Log("No purchase " + level);
        LogEvent("purchase_refuse", level);
    }
}
