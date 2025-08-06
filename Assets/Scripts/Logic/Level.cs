using System.Collections.Generic;
using System.Linq;
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

    public List<int> GetIndexes() => Questions.Select(question => question.GetID()).ToList();
    public IQuestion GetQuestionByID(int id) => Questions.FirstOrDefault(question => question.GetID() == id);
}
