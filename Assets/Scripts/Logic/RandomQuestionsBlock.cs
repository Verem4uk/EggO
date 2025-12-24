using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "RandomBlock", menuName = "SO/RandomBlock")]
public class RandomQuestionsBlock : LevelsElement
{
    [SerializeField]
    public int AmountForOneSession = 5;

    [SerializeField]
    public Question[] Questions;

    public override IQuestion GetNextElement(List<int> exceptIndexes = null)
    {
        if (Questions == null || Questions.Length == 0)
            return null;

        var availableQuestions = Questions
            .Where(q => exceptIndexes == null || !exceptIndexes.Contains(q.ID))
            .ToArray();

        if (availableQuestions.Length == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, availableQuestions.Length);
        return availableQuestions[randomIndex];
    }
}