using UnityEngine;

public class Saver
{
    private static Saver instance;
    private Saver() { }

    public static Saver Instance => instance ??= new Saver();

    public void UpdateLanguage(int value) => PlayerPrefs.SetInt("LanguageIdentifier", value);

    public int GetLanguageIdentifier() => PlayerPrefs.GetInt("LanguageIdentifier");
}
