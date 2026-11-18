// Minimal substitutes for running real runtime logic without the Unity editor.
// No network requests, rendering or Unity serialization are performed here.
namespace UnityEngine
{
    public class Object
    {
        public static int Destroyed;
        public static void Destroy(Object value) { Destroyed++; }
    }
    public class ScriptableObject : Object { public string name; }
    public class SerializeField : Attribute { }
    public class TextAreaAttribute : Attribute { }
    public class MinAttribute : Attribute { public MinAttribute(int value) { } }
    public class CreateAssetMenuAttribute : Attribute { public string fileName; public string menuName; }
    public class AudioClip { }
    public struct Color { }
    public struct Rect { public Rect(float x, float y, float width, float height) { } }
    public struct Vector2 { public Vector2(float x, float y) { } }
    public class Texture2D { public int width = 1; public int height = 1; }
    public class Sprite : Object
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
        public static readonly List<string> Messages = new();
        public static void Log(object message) => Messages.Add(message.ToString());
    }
    public static class PlayerPrefs
    {
        private static readonly Dictionary<string, int> Values = new();
        public static void SetInt(string key, int value) => Values[key] = value;
        public static int GetInt(string key) => Values.TryGetValue(key, out int value) ? value : 0;
        public static void DeleteAll() => Values.Clear();
    }
}
namespace UnityEngine.ResourceManagement.AsyncOperations
{
    public enum AsyncOperationStatus { None, Succeeded, Failed }
    public sealed class OperationState<T>
    {
        public T Result;
        public AsyncOperationStatus Status;
        public bool IsDone, Valid = true;
        public Action<AsyncOperationHandle<T>> Completed;
    }
    public struct AsyncOperationHandle<T>
    {
        internal OperationState<T> State;
        public bool IsDone => State.IsDone;
        public T Result => State.Result;
        public AsyncOperationStatus Status => State.Status;
        public Exception OperationException => new InvalidOperationException("Test Addressables error");
        public bool IsValid() => State.Valid;
        public event Action<AsyncOperationHandle<T>> Completed
        {
            add { State.Completed += value; }
            remove { State.Completed -= value; }
        }
    }
}
namespace UnityEngine.AddressableAssets
{
    using UnityEngine.ResourceManagement.AsyncOperations;
    public static class Addressables
    {
        public static int Releases;
        public static bool Defer;
        private static readonly Queue<Action<bool>> Pending = new();
        public static void Reset()
        {
            Releases = 0;
            UnityEngine.Object.Destroyed = 0;
            Defer = false;
            Pending.Clear();
        }
        public static AsyncOperationHandle<T> LoadAssetAsync<T>(string key) where T : new()
        {
            var state = new OperationState<T> { Result = new T() };
            var handle = new AsyncOperationHandle<T> { State = state };
            if (Defer)
                Pending.Enqueue(success =>
                {
                    state.IsDone = true;
                    state.Status = success ? AsyncOperationStatus.Succeeded : AsyncOperationStatus.Failed;
                    state.Completed?.Invoke(handle);
                });
            else
            {
                state.IsDone = true;
                state.Status = AsyncOperationStatus.Succeeded;
            }
            return handle;
        }
        public static void CompleteNext(bool success) => Pending.Dequeue()(success);
        public static void Release<T>(AsyncOperationHandle<T> handle)
        {
            if (!handle.State.Valid) throw new Exception("Double release");
            Releases++;
            handle.State.Valid = false;
        }
    }
}
