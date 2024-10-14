public static class Root
{
    private static Saver Saver;
    public static QuestionSet Questions { private set; get; }
    public static Session CurrentSession { private set; get; }

    public static void Initialize(QuestionSet questions)
    {
        Saver = new Saver();
        //Saver.Clear(); //For clear playerPrefs
        Questions = questions;
        CurrentSession = new Session(Saver.Load());
    }

    public static int GetLanguageIndex()
    {
        return Saver.GetLanguageIdentifier();
    }

    public static void ChangeLanguage(int index)
    {
        Saver.SetLanguage(index);
    }
    
    public static void Save()
    {
        Saver.Save(CurrentSession.Data);
    }
}
