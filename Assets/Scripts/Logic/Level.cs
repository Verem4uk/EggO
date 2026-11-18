using System;
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
}
