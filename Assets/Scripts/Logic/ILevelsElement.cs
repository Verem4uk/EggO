using System.Collections.Generic;
using UnityEngine;

public abstract class LevelsElement : ScriptableObject
{
    public virtual int QuestionCount => 1;

    public abstract IQuestion GetNextElement();
    public abstract void PrepareQuestions();
}
