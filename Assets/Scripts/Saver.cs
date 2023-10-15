using UnityEngine;

public class Saver
{
    private static Saver instance;
    private Saver() { }

    public static Saver Instance => instance ??= new Saver();

    public void UpdateLanguage(int value) => PlayerPrefs.SetInt("LanguageIdentifier", value);

    public int GetLanguageIdentifier() => PlayerPrefs.GetInt("LanguageIdentifier");
    
    public bool HasUnfinishedSession() => PlayerPrefs.GetInt("HasUnfinishedSession", 0) == 1;
    
    public void Save(GameCore.GameProcessStructure gameParameters)
    {
        PlayerPrefs.SetString("GameParameters", JsonUtility.ToJson(gameParameters));
        PlayerPrefs.SetInt("HasUnfinishedSession", 1);
        PlayerPrefs.Save();
    }

    public GameCore.GameProcessStructure Load()
    {
        string jsonString = PlayerPrefs.GetString("GameParameters", "");
        return !string.IsNullOrEmpty(jsonString) ? 
            JsonUtility.FromJson<GameCore.GameProcessStructure>(jsonString) : 
            new GameCore.GameProcessStructure();
    }

    public void Clear()
    {
        PlayerPrefs.SetInt("HasUnfinishedSession", 0);
        PlayerPrefs.Save();
    }
}
