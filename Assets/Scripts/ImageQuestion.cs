using UnityEngine;

[CreateAssetMenu(fileName = "ImageQuestion", menuName = "SO/ImageQuestion")]
public class ImageQuestion : ScriptableObject, IImageQuestion
{
    [SerializeField] 
    private Sprite Image;
    public Sprite GetImage() => Image;
    public int GetID() => int.Parse(name);
    public string GetText() => Root.Questions.QuestionBeforeImage.GetText();
}
