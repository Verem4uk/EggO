using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Session
{
    [System.Serializable]
    public struct SaveData
    {
        public Dictionary<string[],int> Sessions;
    }

    public SaveData Data { private set; get; }
    public string[] Names;
    public Level Level;
    public List<int> indexes;
    public int CurrentIndex;
                
    public Session(int level)
    {   
        Level = Root.Questions[level];
        indexes = Level.GetIndexes();

        CurrentIndex = 0;
    }
    
    public IQuestion GetQuestion()
    {
        if(CurrentIndex >= indexes.Count)
        {
            return null; //tne end of the session
        }

        var question = Level.GetQuestionByID(CurrentIndex);
        CurrentIndex++;
        return question;
    }
}


