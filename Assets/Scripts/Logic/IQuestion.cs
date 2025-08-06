using UnityEngine;

public interface IQuestion
{
    public int GetID();
    public string GetText();

    public bool HasImage();
    public Sprite GetImage();
}


