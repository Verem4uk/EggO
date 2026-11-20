#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class PortraitLayoutInstaller
{
    public static void Run()
    {
        try
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
            var roots = scene.GetRootGameObjects();
            GameObject Root(string name) => roots.Single(root => root.name == name);
            var global = Frame(Root("Canvas"), child => child.name != "Background");
            var background = Root("Canvas").transform.Find("Background");
            var aspect = background.GetComponent<AspectRatioFitter>();
            if (aspect != null) aspect.enabled = false;
            Stretch((RectTransform)background);
            var controls = global.ContentRect.Find("CanvasButtons");
            Stretch((RectTransform)controls);
            ConfigureScreen(global, PortraitScreenLayout.ScreenKind.SharedControls, null, null,
                controls.Find("Dropdown (1)") as RectTransform, controls.Find("SoundButton (1)") as RectTransform, null);

            var menu = Frame(Root("MainMenuScreen"));
            ConfigureScreen(menu, PortraitScreenLayout.ScreenKind.Menu,
                menu.ContentRect.Find("Logo") as RectTransform,
                menu.ContentRect.Find("PlayButton") as RectTransform, null,
                menu.ContentRect.Find("Button") as RectTransform, null);

            var map = Frame(Root("RoadMapScreen"));
            var art = map.ContentRect.Find("MapArt") as RectTransform;
            if (art == null)
            {
                var children = map.ContentRect.Cast<Transform>().ToArray();
                art = Rect("MapArt", map.ContentRect);
                art.anchorMin = art.anchorMax = new Vector2(0.5f, 0.5f);
                art.sizeDelta = new Vector2(800f, 800f * 2400f / 1080f);
                foreach (var child in children) child.SetParent(art, false);
            }
            ConfigureScreen(map, PortraitScreenLayout.ScreenKind.RoadMap, art, null, null, null, null);

            var session = Frame(Root("SessionScreen"));
            var sessionBackground = session.ContentRect.Find("Background");
            Stretch((RectTransform)sessionBackground);
            var dynamic = sessionBackground.Find("DynamicElements");
            Stretch((RectTransform)dynamic);
            var questionCanvas = dynamic.Find("CanvasQuestion");
            Stretch((RectTransform)questionCanvas);
            questionCanvas.GetComponent<VerticalLayoutGroup>().enabled = false;
            var text = questionCanvas.GetComponentInChildren<Text>(true);
            var questionArea = TextArea(text, questionCanvas, "QuestionTextViewport");
            var grid = questionCanvas.GetComponentInChildren<GridLayoutGroup>(true);
            foreach (var image in grid.GetComponentsInChildren<Image>(true)) image.preserveAspect = true;
            var nav = sessionBackground.Find("CanvasButtons");
            Stretch((RectTransform)nav);
            var sessionLayout = Get<ResponsiveSessionLayout>(session.gameObject);
            Assign(sessionLayout, "Frame", session);
            Assign(sessionLayout, "Question", questionArea);
            Assign(sessionLayout, "ImageGrid", grid);
            Assign(sessionLayout, "Next", nav.Find("NextButton"));
            Assign(sessionLayout, "Back", nav.Find("BackButton"));
            Assign(sessionLayout, "Exit", nav.Find("ExitButton"));
            var zoom = sessionBackground.Find("ScaledImageCanvas") ??
                session.SafeAreaRect.Find("ScaledImageCanvas");
            zoom.SetParent(session.SafeAreaRect, false);
            Stretch((RectTransform)zoom);
            ((RectTransform)zoom).offsetMin = new Vector2(24f, 24f);
            ((RectTransform)zoom).offsetMax = new Vector2(-24f, -24f);
            zoom.GetComponent<Image>().preserveAspect = true;

            var warning = Frame(Root("WarningPanel"));
            var warningText = warning.ContentRect.GetComponentInChildren<Text>(true);
            var warningArea = TextArea(warningText, warning.ContentRect, "WarningTextViewport");
            ConfigureScreen(warning, PortraitScreenLayout.ScreenKind.Warning, null, null,
                warning.ContentRect.Find("BackButton") as RectTransform,
                warning.ContentRect.Find("ExitButton") as RectTransform, warningArea);

            var coffee = Frame(Root("CoffeeScreen"));
            var coffeeText = coffee.ContentRect.GetComponentInChildren<Text>(true);
            var coffeeArea = TextArea(coffeeText, coffee.ContentRect, "CoffeeTextViewport");
            var coffeeArt = coffee.ContentRect.Find("CoffeeImage") as RectTransform;
            coffeeArt.localScale = Vector3.one;
            coffeeArt.GetComponent<Image>().preserveAspect = true;
            var coffeeButtons = coffee.ContentRect.Find("CoffeeButtons");
            Stretch((RectTransform)coffeeButtons);
            ConfigureScreen(coffee, PortraitScreenLayout.ScreenKind.Coffee, coffeeArt, null,
                coffeeButtons.Find("YesButton") as RectTransform,
                coffeeButtons.Find("NoButton") as RectTransform, coffeeArea);

            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("PASS: portrait layout installed; original asset GUIDs and button bindings preserved.");
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    private static T Get<T>(GameObject owner) where T : Component =>
        owner.GetComponent<T>() ?? owner.AddComponent<T>();

    private static PortraitCanvas Frame(GameObject root, Func<Transform, bool> move = null)
    {
        var canvas = root.GetComponent<Canvas>();
        var scaler = Get<CanvasScaler>(root);
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800f, 1400f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
        var frame = Get<PortraitCanvas>(root);
        if (frame.ContentRect != null) return frame;
        var children = root.transform.Cast<Transform>().ToArray();
        var safe = Rect("SafeArea", root.transform);
        Stretch(safe);
        var content = Rect("PortraitContent", safe);
        Stretch(content);
        foreach (var child in children)
            if (move == null || move(child)) child.SetParent(content, false);
        Assign(frame, "SafeArea", safe);
        Assign(frame, "Content", content);
        return frame;
    }

    private static PortraitTextArea TextArea(Text text, Transform parent, string name)
    {
        var area = text.GetComponentInParent<PortraitTextArea>(true);
        if (area == null)
        {
            var rect = Rect(name, parent);
            area = rect.gameObject.AddComponent<PortraitTextArea>();
            text.fontSize = Mathf.RoundToInt(text.fontSize * text.rectTransform.localScale.x);
            text.rectTransform.SetParent(rect, false);
        }
        foreach (var duplicate in parent.GetComponentsInChildren<PortraitTextArea>(true))
            if (duplicate != area && duplicate.name == name &&
                !text.transform.IsChildOf(duplicate.transform))
                UnityEngine.Object.DestroyImmediate(duplicate.gameObject);
        // Questions, warnings and coffee copy are fitted in place, never scrollable.
        var scroll = area.GetComponent<ScrollRect>();
        if (scroll != null) UnityEngine.Object.DestroyImmediate(scroll);
        foreach (var bar in area.GetComponentsInChildren<Scrollbar>(true))
            UnityEngine.Object.DestroyImmediate(bar.gameObject);
        var mask = area.GetComponent<RectMask2D>();
        if (mask != null) UnityEngine.Object.DestroyImmediate(mask);
        var hit = area.GetComponent<Image>();
        if (hit != null) UnityEngine.Object.DestroyImmediate(hit);
        text.rectTransform.localScale = Vector3.one;
        text.raycastTarget = false;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.resizeTextForBestFit = false;
        Assign(area, "Text", text);
        var serialized = new SerializedObject(area);
        serialized.FindProperty("maximumFontSize").intValue = text.fontSize;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return area;
    }

    private static void ConfigureScreen(PortraitCanvas frame, PortraitScreenLayout.ScreenKind kind,
        RectTransform primary, RectTransform secondary, RectTransform left, RectTransform right,
        PortraitTextArea text)
    {
        var layout = Get<PortraitScreenLayout>(frame.gameObject);
        Assign(layout, "Frame", frame);
        var serialized = new SerializedObject(layout);
        serialized.FindProperty("Kind").enumValueIndex = (int)kind;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        Assign(layout, "Primary", primary);
        Assign(layout, "Secondary", secondary);
        Assign(layout, "LeftButton", left);
        Assign(layout, "RightButton", right);
        Assign(layout, "TextArea", text);
    }

    private static void Assign(Component target, string field, UnityEngine.Object value)
    {
        var serialized = new SerializedObject(target);
        serialized.FindProperty(field).objectReferenceValue = value;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static RectTransform Rect(string name, Transform parent)
    {
        var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        return rect;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }
}
#endif
