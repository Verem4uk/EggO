using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "ImageQuestionCategory", menuName = "SO/ImageQuestionCategory")]
public class ImagesQuestionsPack : ScriptableObject
{
    [SerializeField]
    public List<ImageQuestion> Questions;

    public List<int> GetIndexes() => Questions.Select(question => question.GetID()).ToList();

    public IImageQuestion GetQuestionByID(int id)
    {
        foreach (var question in Questions)
        {
            if (question.GetID() == id)
            {
                return question;
            }
        }
        
        Debug.LogError("There is no an element with ID: "+id);
        return null;
    }
}
