using UnityEngine;

[CreateAssetMenu(fileName = "Level", menuName = "SO/Level")]
public class Level : ScriptableObject
{
    [SerializeField] 
    public int ID;

    [SerializeField]
    public Sprite Icon;

    [SerializeField]
    public Sprite BackGround;

    [SerializeField]
    public AudioClip Audio;

    [SerializeField]
    public LevelsElement[] Elements;
}
