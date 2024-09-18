using System.Collections.Generic;
using UnityEngine;

public static class Root
{
    public static Saver Saver { private set; get; }
    public static QuestionsPack BaseQuestions { private set; get; }
    public static ImagesQuestionsPack ImagesQuestions { private set; get; }
    public static Session CurrentSession { private set; get; }
    
    public static Question LastQuestion { private set; get; }

    public static void Initialize(QuestionsPack questions, ImagesQuestionsPack imagesQuestions, Question lastQuestion,
        int imageProbability)
    {
        BaseQuestions = questions;
        ImagesQuestions = imagesQuestions;
        LastQuestion = lastQuestion;
        Saver = new Saver();
        //Saver.Clear();
        CurrentSession = new Session(Saver.Load(), BaseQuestions.GetIndexes(), ImagesQuestions.GetIndexes(), imageProbability);
        if (CurrentSession != null)
        {
            Debug.Log("Session is not null");
        }
    }
    
    public static void Save()
    {
        Saver.Save(CurrentSession.Data);
    }
}
