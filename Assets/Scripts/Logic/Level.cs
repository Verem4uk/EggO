using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "SO/Level")]
public class Level : ScriptableObject
{
    [SerializeField] 
    public int ID;
    
    [SerializeField]
    public List<Question> Questions;

    [SerializeField]
    public int startRandomIndex;

    [SerializeField]
    public int endRandomIndex;

    public IQuestion GetQuestionByID(int id) => Questions[id];
}
