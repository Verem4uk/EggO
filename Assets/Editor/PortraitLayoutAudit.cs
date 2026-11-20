#if UNITY_EDITOR
using System;
using System.Linq;
using System.Reflection;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class PortraitLayoutAudit
{
    public static void Run()
    {
        try
        {
            var scene = EditorSceneManager.OpenScene("Assets/Scenes/MainScene.unity");
            var output = new StringBuilder();
            foreach (var root in scene.GetRootGameObjects())
                foreach (var rect in root.GetComponentsInChildren<RectTransform>(true))
                {
                    string path = rect.name;
                    for (var parent = rect.parent; parent != null; parent = parent.parent) path = parent.name + "/" + path;
                    var components = string.Join(",", rect.GetComponents<Component>()
                        .Where(c => c != null).Select(c => c.GetType().Name));
                    var text = rect.GetComponent<Text>();
                    var tmp = rect.GetComponent<TMPro.TMP_Text>();
                    output.AppendLine(path + " | anchor=" + rect.anchorMin + ":" + rect.anchorMax +
                        " pos=" + rect.anchoredPosition + " size=" + rect.sizeDelta + " scale=" + rect.localScale +
                        " pivot=" + rect.pivot + " | " + components +
                        (text != null ? " | FONT=" + text.fontSize + " TEXT=" + text.text.Replace("\n", " ").Substring(0, Math.Min(70, text.text.Length)) : "") +
                        (tmp != null ? " | TMP=" + tmp.fontSize : ""));
                }
            var assembly = typeof(Editor).Assembly;
            var sizesType = assembly.GetType("UnityEditor.GameViewSizes");
            var singleton = typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
            var sizes = singleton.GetProperty("instance", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).GetValue(null);
            var group = sizesType.GetMethod("GetGroup").Invoke(sizes, new object[] { 0 });
            var labels = (string[])group.GetType().GetMethod("GetDisplayTexts").Invoke(group, null);
            var getSize = group.GetType().GetMethod("GetGameViewSize");
            for (int i = 0; i < labels.Length; i++)
            {
                var size = getSize.Invoke(group, new object[] { i });
                output.AppendLine("GAMEVIEW " + i + " " + labels[i] + " " +
                    size.GetType().GetProperty("width").GetValue(size) + "x" +
                    size.GetType().GetProperty("height").GetValue(size));
            }
            var directory = Path.GetFullPath(Path.Combine(Application.dataPath, "../Logs/PortraitLayout"));
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "audit.txt"), output.ToString());
            Debug.Log("PASS: portrait layout audit exported.");
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }
}
#endif
