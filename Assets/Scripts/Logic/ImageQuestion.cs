using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ImageQuestion", menuName = "SO/ImageQuestion")]
public class ImageQuestion : Question
{
    [SerializeField]
    private string[] imageKeys;

    public override IReadOnlyList<string> ImageKeys =>
        Array.AsReadOnly(imageKeys ?? Array.Empty<string>());

    public override bool HasImage() => imageKeys != null && imageKeys.Length > 0;
}
