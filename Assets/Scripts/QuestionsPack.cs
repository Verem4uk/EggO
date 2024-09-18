using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestionCategory", menuName = "SO/QuestionCategory")]
public class QuestionsPack : ScriptableObject
{
    [SerializeField]
    public List<Question> Questions;

    public List<int> GetIndexes() => Questions.Select(question => question.GetID()).ToList();

    public IQuestion GetQuestionByID(int id) => Questions.FirstOrDefault(question => question.GetID() == id);
}
