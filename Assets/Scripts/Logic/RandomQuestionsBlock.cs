using UnityEngine;

[CreateAssetMenu(fileName = "RandomBlock", menuName = "SO/RandomBlock")]
public class RandomQuestionsBlock : LevelsElement
{
    [SerializeField]
    public int AmountForOneSession = 5;

    [SerializeField]
    public QuestionsCollection Collection;
}
