using System.Collections.Generic;

public class Session
{    
    private string[] Names;
    private Level Level;
    private int LevelIndex;

    private int CurrentElementIndex;
    private LevelsElement CurrentElement;
    private List<int> CurrentElementIndexes = new List<int>();
                    
    public Session(int level)
    {   
        Level = Root.Levels[--level];
        LevelIndex = level;
        CurrentElementIndex = 0;
    }

    public string GetCounterInfo()
    {
        return CurrentElement is RandomQuestionsBlock randomBlock
            ? (CurrentElementIndexes.Count).ToString() + "/" + randomBlock.AmountForOneSession.ToString()
            : "";
    }

    public IQuestion GetQuestion()
    {
        if (CurrentElement == null)
        {
            if (CurrentElementIndex >= Level.Elements.Length)
            {
                Saver.SetLevel(++LevelIndex);
                return null; // конец сессии
            }

            CurrentElement = Level.Elements[CurrentElementIndex];
            CurrentElementIndex++;
            CurrentElementIndexes.Clear();
        }
                
        if (CurrentElement is RandomQuestionsBlock randomBlock)
        {            
            if (CurrentElementIndexes.Count >= randomBlock.AmountForOneSession)
            {
                CurrentElement = null;
                return GetQuestion();
            }

            var nextQuestion = randomBlock.GetNextElement(CurrentElementIndexes);
            if (nextQuestion == null)
            {                
                CurrentElement = null;
                return GetQuestion();
            }

            CurrentElementIndexes.Add(nextQuestion.GetID());
            return nextQuestion;
        }

        var question = CurrentElement.GetNextElement(CurrentElementIndexes);
        if (question == null)
        {
            CurrentElement = null;
            return GetQuestion();
        }

        CurrentElementIndexes.Add(question.GetID());
        return question;
    }
}


