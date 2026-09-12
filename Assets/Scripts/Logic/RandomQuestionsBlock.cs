using UnityEngine;

[CreateAssetMenu(fileName = "RandomBlock", menuName = "SO/RandomBlock")]
public class RandomQuestionsBlock : LevelsElement
{
    [SerializeField]
    public int AmountForOneSession = 5;

    [SerializeField]
    public QuestionsCollection Collection;

    private Question[] PreparedQuestions;
    private int nextIndex;

    public override void PrepareQuestions()
    {
        int amount = Mathf.Min(
            AmountForOneSession,
            Collection.AvailableCount);

        PreparedQuestions = new Question[amount];

        for (int i = 0; i < amount; i++)
        {
            Question question = Collection.TakeRandomQuestion();
            PreparedQuestions[i] = question;

            if (question is ImageQuestion imageQuestion)
                imageQuestion.PrepareImagesAsync();
        }

        nextIndex = 0;
    }

    public override IQuestion GetNextElement()
    {
        if (PreparedQuestions == null ||
            nextIndex >= PreparedQuestions.Length)
        {
            return null;
        }

        return PreparedQuestions[nextIndex++];
    }
    public string GetCounterInfo()
    {
        int total = PreparedQuestions?.Length ?? 0;
        return $"{nextIndex}/{total}";
    }
}