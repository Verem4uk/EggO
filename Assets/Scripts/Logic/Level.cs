using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "SO/Level")]
public class Level : ScriptableObject
{
    [SerializeField] 
    public int ID;

    [SerializeField]
    public Sprite Icon;

    [Serializable]
    public class CompletionRequirement
    {
        public LevelsElement Element;

        // Zero means all questions actually prepared for this element.
        [Min(0)]
        public int MinimumQuestions;
    }

    [SerializeField]
    public CompletionRequirement[] CompletionRequirements;

    [SerializeField]
    public LevelsElement[] FinishElements;

    [SerializeField]
    public AudioClip ButtonSound;

    [SerializeField]
    public Color BackgroundColor;

    [SerializeField]
    public Color SmokeColor;

    [SerializeField]
    public Color TextColor;

    [SerializeField]
    public string BackGroundAudio;

    [SerializeField]
    public LevelsElement[] Elements;

    public bool IsCompleted(IReadOnlyDictionary<LevelsElement, int> openedQuestions)
    {
        if (CompletionRequirements == null || CompletionRequirements.Length == 0)
            return false;

        foreach (var requirement in CompletionRequirements)
        {
            if (requirement == null || requirement.Element == null)
                return false;

            int requiredCount = requirement.MinimumQuestions > 0
                ? requirement.MinimumQuestions
                : requirement.Element.QuestionCount;

            if (requiredCount <= 0 ||
                !openedQuestions.TryGetValue(requirement.Element, out int openedCount) ||
                openedCount < requiredCount)
                return false;
        }

        return true;
    }

    public bool IsTheLastQuestion(LevelsElement element) =>
        Elements != null && Elements.Length > 0 && Elements[Elements.Length - 1] == element;
}
