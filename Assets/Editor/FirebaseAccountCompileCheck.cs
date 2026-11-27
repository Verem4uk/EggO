#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Player;
using UnityEngine;

// Compile player-only WebGL branches without building the full WASM player.
public static class FirebaseAccountCompileCheck
{
    public static void Run()
    {
        try
        {
            var settings = new ScriptCompilationSettings
            {
                target = BuildTarget.WebGL,
                group = BuildTargetGroup.WebGL,
                options = ScriptCompilationOptions.None
            };
            var result = PlayerBuildInterface.CompilePlayerScripts(settings, "Library/FirebaseAccountCompile");
            if (result.assemblies == null || !result.assemblies.Any(path => Path.GetFileName(path) == "Assembly-CSharp.dll"))
                throw new InvalidOperationException("WebGL player scripts did not compile.");
            Debug.Log("PASS: WebGL player scripts compiled, including Firebase account callbacks.");
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
    }
}
#endif
