using UnityEngine;

public static class Saver
{    
    public static void SetLanguage(int value) => PlayerPrefs.SetInt("Language", value);
    public static int GetLanguage() => PlayerPrefs.GetInt("Language");

    public static int LastPastLevel { get; private set; }

    private static bool IsTrial;

    public static void InitTrial(bool value)
    {
        IsTrial = value;
    }

    public static void InitLevel(int value)
    {
        PlayerPrefs.SetInt("Level", value);        
    }

    public static int MaxAvailableLevel() => IsTrial ? 2 : GetLevel() + 1;

    public static void SetLevel(int value)
    {
        var savedIndex = GetLevel();
        if(value > savedIndex)
        {
            PlayerPrefs.SetInt("Level", value);
        }
        LastPastLevel = value;
    }

    public static int GetLevel() => PlayerPrefs.GetInt("Level");         
    
    public static void Clear()
    {
        PlayerPrefs.DeleteAll();
    }
}
