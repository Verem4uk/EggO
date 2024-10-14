using UnityEngine;

public class Saver
{
    public void SetLanguage(int value) => PlayerPrefs.SetInt("LanguageIdentifier", value);

    public int GetLanguageIdentifier() => PlayerPrefs.GetInt("LanguageIdentifier");
    
    public void Save(Session.SessionData sessionData)
    {
        PlayerPrefs.SetString("Session", JsonUtility.ToJson(sessionData));
        PlayerPrefs.Save();
    }

    public Session.SessionData Load()
    {
        string jsonString = PlayerPrefs.GetString("Session", "");
        return !string.IsNullOrEmpty(jsonString)
            ? JsonUtility.FromJson<Session.SessionData>(jsonString) : new Session.SessionData();
    }

    public void Clear()
    {
        PlayerPrefs.DeleteAll();
    }
}
