using UnityEngine;

[CreateAssetMenu(fileName = "QuestionsCollection", menuName = "SO/QuestionsCollection")]
public class QuestionsCollection : ScriptableObject
{
    [SerializeField]
    public Question[] Questions;
}

