#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using DG.Tweening;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class MapIconSmokeTests
{
    public static void Run()
    {
        try
        {
            int checks = Validate();
            Debug.Log($"PASS: {checks} map icon state and opacity checks.");
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
    }

    public static int Validate()
    {
        if (!Application.isPlaying)
            typeof(DOTween).GetField("initialized", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, true);
        int checks = MapIconStateTests.Validate();
        void Check(bool condition, string message)
        {
            checks++;
            if (!condition) throw new InvalidOperationException(message);
        }

        var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
        var map = scene.GetRootGameObjects().SelectMany(root =>
            root.GetComponentsInChildren<RoadMapScreen>(true)).Single();
        map.gameObject.SetActive(true);

        for (int completed = 0; completed <= map.pointsMap.Length; completed++)
        {
            foreach (var point in map.pointsMap)
            {
                int id = Field<int>(point, "ID");
                var button = Field<Button>(point, "ActiveEggO");
                var image = button.image != null ? button.image : button.GetComponent<Image>();
                var past = Field<Image>(point, "PastEggO");
                var pulse = Field<Pulsation>(point, "Pulsation");
                var originalIcon = image.sprite;
                var completedIcon = past.sprite;
                Check(originalIcon != null && completedIcon != null, "Missing authored map icon.");

                // Use immediate native state transitions for deterministic editor checks.
                button.targetGraphic = image;
                var colors = button.colors;
                colors.fadeDuration = 0f;
                button.colors = colors;
                button.interactable = true;
                button.interactable = false;

                point.ShowSequence(completed).SetUpdate(true).Complete(withCallbacks: true);
                if (id > completed)
                {
                    Check(image.enabled && Mathf.Approximately(image.color.a, 1f) &&
                        Mathf.Approximately(image.canvasRenderer.GetColor().a,
                            button.colors.disabledColor.a * button.colors.colorMultiplier),
                        $"Locked icon {id} must be dimmed at progress {completed}.");
                    Check(!button.interactable && !pulse.enabled, "Locked level became interactive.");
                    Check(!past.gameObject.activeSelf && image.sprite == originalIcon,
                        "A locked level lost its authored sprite or showed the completed character.");
                }
                else
                {
                    Check(!image.enabled && past.gameObject.activeSelf && past.color.a == 1f,
                        "Completed level did not display its character.");
                    Check(past.sprite == completedIcon, "Completed character was replaced.");
                }

                var unlock = point.UnlockSequence(completed);
                if (unlock.Duration() > 0f) unlock.Complete(withCallbacks: true);
                else unlock.Kill();
                if (id == completed + 1)
                {
                    Check(button.interactable && pulse.enabled, "Available level did not unlock.");
                    Check(Mathf.Approximately(image.canvasRenderer.GetColor().a, 1f),
                        "Clickable level did not become bright.");
                    Check(button.transition == Selectable.Transition.ColorTint,
                        "Available level lost its authored button interaction.");
                }
                else if (id > completed)
                    Check(!button.interactable && Mathf.Approximately(image.canvasRenderer.GetColor().a,
                            button.colors.disabledColor.a * button.colors.colorMultiplier),
                        "Unavailable icon became bright or unlocked while another point opened.");

                point.HideSequence().Complete(withCallbacks: true);
                Check((id > completed ? image : past).color.a == 0f, "Icon hide animation did not finish.");
            }
        }
        DOTween.KillAll();
        return checks;
    }

    private static T Field<T>(object instance, string name) =>
        (T)instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
}
#endif
