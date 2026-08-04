using UnityEngine;

public static class Saver
{    
    public static void SetLanguage(int value) => PlayerPrefs.SetInt("Language", value);
    public static int GetLanguage() => PlayerPrefs.GetInt("Language");

    public static int LastPastLevel { get; private set; }

    public static void InitLevel(int value)
    {
        PlayerPrefs.SetInt("Level", value);        
    }

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
