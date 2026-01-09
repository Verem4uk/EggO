using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "SO/Level")]
public class Level : ScriptableObject
{
    [SerializeField] 
    public int ID;

    [SerializeField]
    public Sprite Icon;

    [SerializeField]
    public AudioClip ButtonSound;

    [SerializeField]
    public Color BackgroundColor;

    [SerializeField]
    public Color SmokeColor;

    [SerializeField]
    public Color TextColor;

    [SerializeField]
    public AudioClip BackGroundAudio;

    [SerializeField]
    public LevelsElement[] Elements;
}
