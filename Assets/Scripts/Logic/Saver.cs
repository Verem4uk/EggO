using UnityEngine;
using Newtonsoft.Json;

public class Saver
{
    public void SetLanguage(int value) => PlayerPrefs.SetInt("LanguageIdentifier", value);

    public int GetLanguageIdentifier() => PlayerPrefs.GetInt("LanguageIdentifier");
    
    public void Save(Session.SaveData sessionData)
    {
        string json = JsonConvert.SerializeObject(sessionData);
        PlayerPrefs.SetString("Session", json);
        PlayerPrefs.Save();
    }

    public Session.SaveData Load()
    {
        string jsonString = PlayerPrefs.GetString("Session", "");
        return !string.IsNullOrEmpty(jsonString)
            ? JsonConvert.DeserializeObject<Session.SaveData>(jsonString)
            : new Session.SaveData();
    }
    
    public void Clear()
    {
        PlayerPrefs.DeleteAll();
    }
}
