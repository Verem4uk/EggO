
public class Localization
{
    private static Localization instance;
    private Localization() { }

    public static Localization Instance => instance ??= new Localization();

    public void ChangeLanguage(int position) => Saver.Instance.UpdateLanguage(position);

    public string GetTextAccordingLanguage(Question logicQuestion)
    {
        return Saver.Instance.GetLanguageIdentifier() switch
        {
            1 => logicQuestion.GetRussianText,
            2 => logicQuestion.GetPolishText,
            _ => logicQuestion.GetEnglishText
        };
    }
    
    public string GetTextAccordingLanguage(TextTranslation textTranslation)
    {
        return Saver.Instance.GetLanguageIdentifier() switch
        {
            1 => textTranslation.GetRussianText,
            2 => textTranslation.GetPolishText,
            _ => textTranslation.GetEnglishText
        };
    }
}
