public static class Root
{    
    public static Level[] Levels { private set; get; }    
    

    public static void Initialize(Level[] levels)
    {       
        Levels = levels;        
    }

    public static int GetLanguageIndex()
    {
        return Saver.GetLanguage();
    }

    public static void ChangeLanguage(int index)
    {
        Saver.SetLanguage(index);
    }    
}
