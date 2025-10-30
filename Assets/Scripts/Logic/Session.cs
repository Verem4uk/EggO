using UnityEngine;

public class Session
{    
    private string[] Names;
    private Level Level;
    private int LevelIndex;
    private int CurrentIndex;
                
    public Session(int level)
    {   
        Level = Root.Levels[--level];
        LevelIndex = level;
        CurrentIndex = 0;
    }
    
    public IQuestion GetQuestion()
    {
        Debug.Log("Count " + Level.Questions.Count);
        if (CurrentIndex >= Level.Questions.Count)
        {
            Saver.SetLevel(++LevelIndex);
            return null; //tne end of the session
        }

        var question = Level.GetQuestionByID(CurrentIndex);
        CurrentIndex++;
        return question;
    }
}


