using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ImageQuestion", menuName = "SO/ImageQuestion")]
public class ImageQuestion : Question
{
    [SerializeField] 
    private string[] imageKeys;

    private Sprite[] LoadedSprites;

    public override bool HasImage() => imageKeys != null && imageKeys.Length > 0;

    public async void PrepareImagesAsync()
    {        
        Debug.Log($"Start loading {imageKeys.Length} sprites...");

        List<Sprite> sprites = new List<Sprite>();
        for (int i = 0; i < imageKeys.Length; i++)
        {
            string key = imageKeys[i];
            Debug.Log($"Loading sprite {i + 1}/{imageKeys.Length}: {key}");
            Texture2D tex = await Addressables.LoadAssetAsync<Texture2D>(key).Task;
            Sprite sprite = Sprite.Create(tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f));
            sprites.Add(sprite);
            Debug.Log($"Loaded sprite {i + 1}: {sprite.name}");
        }

        LoadedSprites = sprites.ToArray();
        Debug.Log("All sprites loaded successfully");
    }

    public override Sprite[] GetImages() => LoadedSprites ?? new Sprite[0];
}
