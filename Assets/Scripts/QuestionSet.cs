using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestionSet", menuName = "SO/QuestionSet")]
public class QuestionSet : ScriptableObject
{
    public QuestionsPack FreeQuestionsPack;
    
    public ImagesPack FreeImagesPack;

    public List<QuestionsPack> QuestionsPacks;

    public List<ImagesPack> ImagesPacks;

    public Question QuestionBeforeImage;

    public Question QuestionAfterImage;
    public Question LastQuestionInSession;
    public int ImageProbability = 5;
}
