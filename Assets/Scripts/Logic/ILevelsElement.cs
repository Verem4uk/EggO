using System.Collections.Generic;
using UnityEngine;

public abstract class LevelsElement : ScriptableObject
{
    public abstract IQuestion GetNextElement();
    public abstract void PrepareQuestions();
}