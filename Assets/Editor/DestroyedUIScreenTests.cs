#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using DG.Tweening;
using UnityEditor;
using UnityEngine;

public static class DestroyedUIScreenTests
{
    public static void Run()
    {
        try
        {
            int checks = Validate();
            Debug.Log($"PASS: {checks} destroyed-UI lifecycle checks.");
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
        // DOTween's public Init is disabled in Edit Mode; advance no live update loop.
        if (!Application.isPlaying)
            typeof(DOTween).GetField("initialized", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, true);
        int checks = 0;
        void Check(bool condition, string message)
        {
            checks++;
            if (!condition) throw new InvalidOperationException(message);
        }

        foreach (bool destroyScreensFirst in new[] { true, false })
        {
            var owned = new List<GameObject>();
            GameObject Owner(string name)
            {
                var owner = new GameObject(name);
                owner.SetActive(false);
                owned.Add(owner);
                return owner;
            }
            CanvasGroupAutoFadeWithImages Fader(string name)
            {
                var owner = Owner(name);
                owner.AddComponent<CanvasGroup>();
                var fader = owner.AddComponent<CanvasGroupAutoFadeWithImages>();
                fader.SetInput(true); // Cache the native CanvasGroup before destroying it.
                return fader;
            }

            var controller = Owner("Lifecycle controller").AddComponent<Controller>();
            var menu = Fader("Lifecycle menu");
            var session = Fader("Lifecycle session");
            var coffee = Fader("Lifecycle coffee");
            var roadFade = Fader("Lifecycle map");
            var road = roadFade.gameObject.AddComponent<RoadMapScreen>();
            road.pointsMap = Array.Empty<PointMap>();
            Set(road, "Canvas", roadFade);
            var buttons = Owner("Lifecycle shared buttons").AddComponent<CanvasGroup>();
            Set(controller, "Configured", true);
            Set(controller, "MenuFade", menu);
            Set(controller, "SessionFade", session);
            Set(controller, "CoffeeFade", coffee);
            Set(controller, "RoadMap", road);
            Set(controller, "SharedButtons", buttons);
            bool cancelled = false;
            var transition = DOTween.Sequence().AppendInterval(1f).Pause().OnKill(() => cancelled = true);
            Set(controller, "Transition", transition);

            try
            {
                if (destroyScreensFirst)
                {
                    UnityEngine.Object.DestroyImmediate(menu.gameObject);
                    UnityEngine.Object.DestroyImmediate(session.gameObject);
                    UnityEngine.Object.DestroyImmediate(coffee.gameObject);
                    UnityEngine.Object.DestroyImmediate(roadFade);
                    Check(!ReferenceEquals(menu, null) && menu == null,
                        "Fixture did not preserve a managed wrapper for a destroyed Unity component.");
                    // Even a stale callback is safe at the presentation boundary.
                    menu.SetInput(false);
                    menu.SetVisibleImmediate(false);
                    road.SetInput(false);
                    Check(true, "Destroyed UI callbacks should not access native objects.");
                }

                Invoke(controller, "OnDisable");
                Check(cancelled && !controller.IsTransitioning, "Teardown did not cancel its active transition.");
                Check(!buttons.interactable && !buttons.blocksRaycasts,
                    "Surviving shared controls remained interactive.");

                if (!destroyScreensFirst)
                {
                    Check(!menu.GetComponent<CanvasGroup>().interactable &&
                        !session.GetComponent<CanvasGroup>().interactable &&
                        !coffee.GetComponent<CanvasGroup>().interactable,
                        "Live screens were not disabled when the controller stopped first.");
                }

                UnityEngine.Object.DestroyImmediate(road.gameObject);
                UnityEngine.Object.DestroyImmediate(buttons.gameObject);
                Invoke(controller, "OnDisable");
                Check(!controller.IsTransitioning, "Repeated teardown accessed destroyed map/buttons.");
                Invoke(controller, "OnDestroy");
                Check(true, "Controller destruction must remain safe after its screens disappear.");
            }
            finally
            {
                transition.Kill();
                foreach (var owner in owned)
                    if (owner != null) UnityEngine.Object.DestroyImmediate(owner);
            }
        }
        return checks;
    }

    private static void Set(object instance, string name, object value) =>
        instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, value);

    private static void Invoke(object instance, string name) =>
        instance.GetType().GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(instance, null);
}
#endif
