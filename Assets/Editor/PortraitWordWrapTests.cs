#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class PortraitWordWrapTests
{
    public static void Run()
    {
        try
        {
            int checks = Validate();
            Debug.Log($"PASS: {checks} whole-word font fitting checks.");
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    public static int Validate()
    {
        var root = new GameObject("Word wrap fixture", typeof(RectTransform));
        var owner = new GameObject("Text area", typeof(RectTransform));
        owner.transform.SetParent(root.transform, false);
        var textOwner = new GameObject("Text", typeof(RectTransform), typeof(Text));
        textOwner.transform.SetParent(owner.transform, false);
        var area = owner.AddComponent<PortraitTextArea>();
        var text = textOwner.GetComponent<Text>();
        text.font = AssetDatabase.LoadAssetAtPath<Font>("Assets/Resources/PlayfairDisplay-Bold.otf");
        text.alignment = TextAnchor.UpperCenter;
        text.supportRichText = false;
        typeof(PortraitTextArea).GetField("Text", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(area, text);
        int checks = 0;
        void Check(bool condition, string message)
        {
            checks++;
            if (!condition) throw new InvalidOperationException(message);
        }
        try
        {
            foreach (float width in new[] { 200f, 300f, 480f, 640f })
            foreach (float height in new[] { 180f, 700f })
            foreach (var message in new[]
            {
                "Какие напитки предпочитаете?",
                "Если бы у вас была возможность переместиться во времени, что бы вы сделали?",
                "В чём вы черпаете энергию и находите вдохновение?",
                "What would you do if you had the opportunity to travel through time?",
                "Jakie napoje preferujesz?",
                "odpowiedzialność i nieprzewidywalność",
                "extraordinarily unpredictable circumstances",
                "Самосовершенствование?",
                "Два\u00a0слова вместе",
                "Первая строка\nпредпочитаете?"
            })
            {
                ((RectTransform)owner.transform).sizeDelta = new Vector2(width, height);
                text.text = message;
                area.SetMaximumFontSize(100);
                area.SetArea(width, height);
                Check(area.Fits, $"Text does not fit: {width}x{height}, {message}");
                var settings = text.GetGenerationSettings(new Vector2(width, height));
                settings.generateOutOfBounds = true;
                using (var generator = new TextGenerator())
                {
                    generator.Populate(message, settings);
                    foreach (var line in generator.lines)
                    {
                        int index = line.startCharIdx;
                        if (index <= 0 || index >= message.Length) continue;
                        Check(char.IsWhiteSpace(message[index - 1]) || char.IsWhiteSpace(message[index]),
                            $"Word split at character {index}, font {text.fontSize}: {message}");
                    }
                }
            }
            // Changing the message must invalidate the cached width measurements.
            ((RectTransform)owner.transform).sizeDelta = new Vector2(300f, 700f);
            text.text = "Да";
            area.SetArea(300f, 700f);
            Check(area.FontSize == 100, "Short text unnecessarily shrank.");
            text.text = "предпочитаете?";
            area.SetArea(300f, 700f);
            Check(area.FontSize < 100 && area.Fits, "Long word did not shrink after a message change.");
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
        return checks;
    }
}
#endif
