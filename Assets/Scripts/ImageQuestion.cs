using UnityEngine;

[CreateAssetMenu(fileName = "ImageQuestion", menuName = "SO/ImageQuestion")]
public class ImageQuestion : Question
{
    [SerializeField] 
    private Sprite Image;
    public override bool HasImage() => true;
    public override Sprite GetImage() => Image;    
}
