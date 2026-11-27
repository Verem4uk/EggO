#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Exercises the real smoke prefab and ResponsiveSessionLayout against a camera
// with the same projection as MainScene. Does not save or change source assets.
public static class SmokeViewportTests
{
    private static int Checks;
    private static void Check(bool condition, string message)
    {
        Checks++;
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Set(object target, string name, object value) =>
        target.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance).SetValue(target, value);

    public static void Run()
    {
        try
        {
            Validate();
            Debug.Log($"PASS: {Checks} smoke viewport checks; phone, Full HD, 4K, ultrawide and resize. Previews: Logs/SmokeViewport");
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    private static void Validate()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var camera = new GameObject("Smoke test camera", typeof(Camera)).GetComponent<Camera>();
        camera.transform.position = new Vector3(0, 0, -10);
        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.white;
        var root = new GameObject("Smoke test canvas", typeof(RectTransform), typeof(Canvas));
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = camera;
        root.transform.localScale = Vector3.one * 0.01f;
        root.transform.position = new Vector3(0, 0, 90);
        var content = new GameObject("PortraitContent", typeof(RectTransform)).GetComponent<RectTransform>();
        content.SetParent(root.transform, false);
        content.sizeDelta = new Vector2(800, 1400);
        var background = new GameObject("Background", typeof(RectTransform)).GetComponent<RectTransform>();
        background.SetParent(content, false);
        background.sizeDelta = content.sizeDelta;
        var frame = root.AddComponent<PortraitCanvas>();
        Set(frame, "Content", content);
        var layout = root.AddComponent<ResponsiveSessionLayout>();
        Set(layout, "Frame", frame);
        var arrange = typeof(ResponsiveSessionLayout).GetMethod("ArrangeSmoke", BindingFlags.NonPublic | BindingFlags.Instance);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/SmokeEffect.prefab");
        var systems = new ParticleSystem[2];
        for (int i = 0; i < systems.Length; i++)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, background);
            instance.transform.localPosition = new Vector3(0, i == 0 ? 1 : -1, 0);
            systems[i] = instance.GetComponent<ParticleSystem>();
            systems[i].useAutoRandomSeed = false;
            systems[i].randomSeed = (uint)(123 + i);
        }
        float originalLifetime = systems[0].main.startLifetimeMultiplier;
        float originalSpeed = systems[0].main.startSpeedMultiplier;
        float originalRate = systems[0].emission.rateOverTimeMultiplier;
        int originalLimit = systems[0].main.maxParticles;
        string directory = Path.GetFullPath("Logs/SmokeViewport");
        Directory.CreateDirectory(directory);

        // Reproduce the bug before the responsive fit: both old streams stop
        // well inside a 1920x1080 viewport despite continuous emission.
        camera.aspect = 1920f / 1080f;
        for (int i = 0; i < systems.Length; i++)
        {
            systems[i].transform.position = camera.ViewportToWorldPoint(new Vector3(-0.04f, i == 0 ? 0.96f : 0.04f, 60));
            systems[i].Simulate(80f, true, true, true);
            Check(FurthestParticleX(systems[i], camera) < 0.7f, "Old Full HD truncation was not reproduced");
        }
        Render(camera, 1920, 1080, Path.Combine(directory, "fullhd-before.png"));

        foreach (var size in new[] { new Vector2Int(1080, 2400), new Vector2Int(390, 844),
            new Vector2Int(1080, 1920), new Vector2Int(1920, 1080),
            new Vector2Int(3840, 2160), new Vector2Int(2560, 1080) })
        {
            camera.aspect = size.x / (float)size.y;
            arrange.Invoke(layout, new object[] { false });
            foreach (var system in systems)
            {
                float lifetime = system.main.startLifetimeMultiplier;
                Check(system.main.startSpeedMultiplier == originalSpeed, "Smoke speed changed");
                Check(system.emission.rateOverTimeMultiplier == originalRate, "Emission rate changed");
                Check(system.main.maxParticles == originalLimit, "Particle cap changed");
                if (size.x < size.y) Check(Mathf.Approximately(lifetime, originalLifetime), "Phone lifetime changed");
                else Check(lifetime > originalLifetime, "Wide-screen lifetime did not extend");
                system.Simulate(lifetime + 2f, true, true, true);
                Check(FurthestParticleX(system, camera) > 1.04f, $"Smoke still dies inside {size}");
                Check(system.particleCount < originalLimit, "Particle cap is truncating emission");
            }
            float fitted = systems[0].main.startLifetimeMultiplier;
            arrange.Invoke(layout, new object[] { false });
            Check(Mathf.Approximately(systems[0].main.startLifetimeMultiplier, fitted), "Repeated fitting compounds lifetime");
            Check(systems.All(system => system.particleCount > 0), "Unchanged layout restarted particles");
            if (size == new Vector2Int(1920, 1080)) Render(camera, size.x, size.y, Path.Combine(directory, "fullhd-after.png"));
            if (size == new Vector2Int(1080, 2400)) Render(camera, size.x, size.y, Path.Combine(directory, "phone-after.png"));
            Debug.Log($"Smoke profile {size}: lifetime={fitted:F2}s, particles={systems[0].particleCount}");
        }
        camera.aspect = 1080f / 2400f;
        arrange.Invoke(layout, new object[] { false });
        Check(systems.All(system => Mathf.Approximately(system.main.startLifetimeMultiplier, originalLifetime)),
            "Returning from ultrawide to phone did not restore authored lifetime");
    }

    private static float FurthestParticleX(ParticleSystem system, Camera camera)
    {
        var particles = new ParticleSystem.Particle[system.main.maxParticles];
        int count = system.GetParticles(particles);
        float furthest = float.NegativeInfinity;
        // Smoke uses Local simulation + Local scaling: Unity ignores the UI
        // ancestors' scale when rendering these positions. TransformPoint would
        // incorrectly shrink the measured travel by the canvas scale.
        var main = system.main;
        var scale = main.scalingMode == ParticleSystemScalingMode.Local
            ? system.transform.localScale : main.scalingMode == ParticleSystemScalingMode.Hierarchy
                ? system.transform.lossyScale : Vector3.one;
        var simulationToWorld = Matrix4x4.TRS(system.transform.position, system.transform.rotation, scale);
        for (int i = 0; i < count; i++)
        {
            var world = main.simulationSpace == ParticleSystemSimulationSpace.Local
                ? simulationToWorld.MultiplyPoint3x4(particles[i].position) : particles[i].position;
            furthest = Mathf.Max(furthest, camera.WorldToViewportPoint(world).x);
        }
        return furthest;
    }

    private static void Render(Camera camera, int width, int height, string path)
    {
        var target = new RenderTexture(width, height, 24);
        var pixels = new Texture2D(width, height, TextureFormat.RGB24, false);
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            pixels.Apply();
            File.WriteAllBytes(path, pixels.EncodeToPNG());
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            UnityEngine.Object.DestroyImmediate(pixels);
        }
    }
}
#endif
