using UnityEngine;

public static class Analytics
{
    public static void StartSession(int level)
    {
        Debug.Log("Session started " + level);
    }

    public static void FinishSession(int level, int duration)
    {
        Debug.Log("Session finished " + level + " " + duration);
    }

    public static void InitRegistration()
    {
        Debug.Log("Init registration");
    }

    public static void SuccessRegistration()
    {
        Debug.Log("Success registration");
    }

    public static void StartPurchase(int level)
    {
        Debug.Log("Start purchase " + level);
    }

    public static void CompletePurchase(int level)
    {
        Debug.Log("Complete purchase " + level);
    }

}
