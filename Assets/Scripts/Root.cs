public static class Root
{
    public static Saver Saver { private set; get; }
    public static QuestionsPack BaseQuestions { private set; get; }
    public static Session CurrentSession { private set; get; }

    public static void Initialize(QuestionsPack questions)
    {
        BaseQuestions = questions;
        Saver = new Saver();
        CurrentSession = new Session(Saver.Load(), BaseQuestions.GetIndexes());
    }
    
    public static void Save()
    {
        Saver.Save(CurrentSession.Data);
    }
}
