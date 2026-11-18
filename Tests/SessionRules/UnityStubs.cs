// Minimal substitutes for running the real session logic without the Unity editor.
// These do not validate rendering, Addressables networking or Unity serialization.
namespace UnityEngine
{
    public class ScriptableObject { public string name; }
    public class SerializeField : Attribute { }
    public class TextAreaAttribute : Attribute { }
    public class MinAttribute : Attribute { public MinAttribute(int value) { } }
    public class CreateAssetMenuAttribute : Attribute
    {
        public string fileName;
        public string menuName;
    }
    public class AudioClip { }
    public struct Color { }
    public struct Rect { public Rect(float x, float y, float width, float height) { } }
    public struct Vector2 { public Vector2(float x, float y) { } }
    public class Texture2D { public int width = 1; public int height = 1; }
    public class Sprite
    {
        public string name = "test";
        public static Sprite Create(Texture2D texture, Rect rect, Vector2 pivot) => new Sprite();
    }
    public static class Time { public static float time; }
    public static class Mathf { public static int Min(int a, int b) => Math.Min(a, b); }
    public static class Random
    {
        private static System.Random Generator = new System.Random(42);
        public static void Reset(int seed) => Generator = new System.Random(seed);
        public static int Range(int min, int max) => Generator.Next(min, max);
    }
    public static class Debug
    {
        public static readonly List<string> Messages = new List<string>();
        public static void Log(object message) => Messages.Add(message.ToString());
    }
    public static class PlayerPrefs
    {
        private static readonly Dictionary<string, int> Values = new Dictionary<string, int>();
        public static void SetInt(string key, int value) => Values[key] = value;
        public static int GetInt(string key) => Values.TryGetValue(key, out int value) ? value : 0;
        public static void DeleteAll() => Values.Clear();
    }
}
namespace UnityEngine.AddressableAssets
{
    public static class Addressables
    {
        public sealed class Handle<T> { public Task<T> Task; }
        public static Handle<T> LoadAssetAsync<T>(string key) where T : new() =>
            new Handle<T> { Task = System.Threading.Tasks.Task.FromResult(new T()) };
    }
}
