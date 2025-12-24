using System.Collections.Generic;
using UnityEngine;

public abstract class LevelsElement : ScriptableObject
{
    public abstract IQuestion GetNextElement(List<int> exceptIndexes = null);
}