#if UNITY_EDITOR
using System;
using System.Reflection;
using DG.Tweening;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class MapIconStateTests
{
    public static void Run()
    {
        try
        {
            int count = Validate();
            Debug.Log($"PASS: {count} locked, clickable and completed icon checks.");
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    public static int Validate()
    {
        if (!Application.isPlaying)
            typeof(DOTween).GetField("initialized", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, true);
        int checks = 0;
        void Check(bool condition, string message)
        {
            checks++;
            if (!condition) throw new InvalidOperationException(message);
        }

        var mapOwner = new GameObject("Map input fixture", typeof(RectTransform), typeof(CanvasGroup));
        var mapInput = mapOwner.GetComponent<CanvasGroup>();
        var owner = new GameObject("Map icon state fixture", typeof(RectTransform), typeof(Image), typeof(Button));
        owner.SetActive(false);
        owner.transform.SetParent(mapOwner.transform, false);
        var point = owner.AddComponent<PointMap>();
        var pulse = owner.AddComponent<Pulsation>();
        pulse.enabled = false;
        var button = owner.GetComponent<Button>();
        var image = owner.GetComponent<Image>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.fadeDuration = 0f;
        button.colors = colors;
        var pastOwner = new GameObject("Completed icon", typeof(RectTransform), typeof(Image));
        pastOwner.transform.SetParent(owner.transform, false);
        var past = pastOwner.GetComponent<Image>();
        var completedButton = pastOwner.AddComponent<Button>();
        completedButton.targetGraphic = past;
        var completedColors = completedButton.colors;
        completedColors.fadeDuration = 0f;
        completedButton.colors = completedColors;
        var labelOwner = new GameObject("Level label", typeof(RectTransform), typeof(TextMeshProUGUI));
        labelOwner.transform.SetParent(owner.transform, false);
        var label = labelOwner.GetComponent<TextMeshProUGUI>();
        label.enabled = false;
        var texture = new Texture2D(2, 2);
        var lockedSprite = Sprite.Create(texture, new Rect(0, 0, 2, 2), Vector2.one * 0.5f);
        var completedSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), Vector2.one * 0.5f);
        image.sprite = lockedSprite;
        past.sprite = completedSprite;
        Set(point, "ID", 2);
        Set(point, "ActiveEggO", button);
        Set(point, "PastEggO", past);
        Set(point, "Text", label);
        Set(point, "Pulsation", pulse);

        try
        {
            // Repeated map opening must preserve the state, including after a reset.
            foreach (int progress in new[] { 0, 1, 2, 0, 1 })
            {
                mapInput.interactable = false;
                point.ShowSequence(progress).Complete(withCallbacks: true);
                if (progress < 2)
                {
                    Check(image.enabled && image.color.a == 1f, "Reveal did not display the locked image.");
                    Check(Mathf.Approximately(image.canvasRenderer.GetColor().a, colors.disabledColor.a),
                        "Unavailable level must use its disabled opacity.");
                    Check(!button.interactable && !pulse.enabled, "Unavailable level became clickable.");
                    Check(image.sprite == lockedSprite && !past.gameObject.activeSelf, "Locked sprite was replaced.");
                    Check(!label.gameObject.activeSelf, "Unavailable level displayed its phrase.");
                }
                else
                {
                    Check(!image.enabled && past.gameObject.activeSelf && past.color.a == 1f,
                        "Completed level did not display its character.");
                    Check(past.sprite == completedSprite, "Completed sprite was replaced.");
                    Check(label.gameObject.activeSelf && label.color.a == 1f,
                        "Completed level lost its phrase.");
                    Check(!pulse.enabled, "Completed character must not pulse.");
                    Color beforeInput = past.canvasRenderer.GetColor();
                    Check(Mathf.Approximately(beforeInput.a, completedColors.normalColor.a),
                        "Blocking completed-level input must not dim its character.");
                    mapInput.interactable = true;
                    Check(past.canvasRenderer.GetColor() == beforeInput,
                        "Completed character changed brightness when input was restored.");
                }

                var unlock = point.UnlockSequence(progress);
                if (progress == 1)
                {
                    unlock.SetUpdate(UpdateType.Manual).Play();
                    float halfDuration = unlock.Duration() * 0.5f;
                    DOTween.ManualUpdate(halfDuration, halfDuration);
                    float alpha = image.canvasRenderer.GetColor().a;
                    Check(alpha > colors.disabledColor.a && alpha < colors.normalColor.a,
                        "Unlock brightness must interpolate instead of jumping to normal.");
                    Check(label.gameObject.activeSelf && label.color.a > 0f && label.color.a < 1f,
                        $"The phrase must fade in alongside the available icon (active={label.gameObject.activeSelf}, alpha={label.color.a}).");
                    Check(!button.interactable && !pulse.enabled,
                        "The available level must finish its reveal before starting pulsation.");
                }
                if (unlock.Duration() > 0f) unlock.Complete(withCallbacks: true);
                else unlock.Kill();
                if (progress == 1)
                {
                    Check(button.interactable && pulse.enabled, "Available level did not unlock.");
                    Check(Mathf.Approximately(image.canvasRenderer.GetColor().a, colors.normalColor.a),
                        "Available level must be bright.");
                    Check(label.gameObject.activeSelf && label.color.a == 1f,
                        "Available level did not display its phrase.");
                }
                else if (progress == 0)
                    Check(Mathf.Approximately(image.canvasRenderer.GetColor().a, colors.disabledColor.a),
                        "Still-locked level became bright.");

                point.HideSequence().Complete(withCallbacks: true);
                Check((progress < 2 ? image : past).color.a == 0f, "Hide did not complete.");
            }
        }
        finally
        {
            DOTween.KillAll();
            UnityEngine.Object.DestroyImmediate(mapOwner);
            UnityEngine.Object.DestroyImmediate(lockedSprite);
            UnityEngine.Object.DestroyImmediate(completedSprite);
            UnityEngine.Object.DestroyImmediate(texture);
        }
        return checks;
    }

    private static void Set(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
}
#endif
