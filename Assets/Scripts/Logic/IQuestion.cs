using System.Collections.Generic;

public interface IQuestion
{
    int GetID();
    string GetText();
    bool HasImage();
    IReadOnlyList<string> ImageKeys { get; }
}


