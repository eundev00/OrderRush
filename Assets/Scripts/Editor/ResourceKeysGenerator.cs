using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEditor;
public class ResourceKeysGenerator
{
    [MenuItem("Tools/ResourceKeysGenerator/CreatePrefabKeys", priority = 0)]
    public static void CreatePrefabKeys()
    {
        EditorUtility.DisplayProgressBar("CreatePrefabKeys", "Working...", 1f);
        Thread.Sleep(1000);

        try
        {
            var path = "Assets/Scripts/Services/ResourceService/PrefabKeys.cs";
            var contents = CreatePrefabKeyContents();
            File.WriteAllText(path, contents);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e);
        }

        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/ResourceKeysGenerator/CreateDataKeys", priority = 1)]
    public static void CreateDataKeys()
    {
        EditorUtility.DisplayProgressBar("CreateDataKeys", "Working...", 1f);
        Thread.Sleep(1000);

        try
        {
            var path = "Assets/Scripts/Services/ResourceService/DataKeys.cs";
            var contents = CreateDataKeyContents();
            File.WriteAllText(path, contents);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e);
        }

        EditorUtility.ClearProgressBar();
    }

    [MenuItem("Tools/ResourceKeysGenerator/CreateAudioKeys", priority = 2)]
    public static void CreateAudioKeys()
    {
        EditorUtility.DisplayProgressBar("CreateAudioKeys", "Working...", 1f);
        Thread.Sleep(1000);

        try
        {
            var path = "Assets/Scripts/Services/ResourceService/AudioKeys.cs";
            var contents = CreateAudioKeyContents();
            File.WriteAllText(path, contents);
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError(e);
        }

        EditorUtility.ClearProgressBar();
    }

    private static string CreatePrefabKeyContents()
    {
        var assetGuids = AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/Prefabs", "Assets/Resources" });
        string[] assetPathList = Array.ConvertAll<string, string>(assetGuids, AssetDatabase.GUIDToAssetPath);
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("   public static class PrefabKeys");
        sb.AppendLine("   {");
        foreach (var assetPath in assetPathList)
        {
            var fileName = Path.GetFileNameWithoutExtension(assetPath);
            sb.AppendLine($"    public const string {fileName} = \"{fileName}\";");
        }
        sb.AppendLine("");
        sb.AppendLine("    public static Dictionary<string, string> PrefabPaths = new Dictionary<string, string>()");
        sb.AppendLine("    {");
        foreach (var assetPath in assetPathList)
        {
            var fileName = Path.GetFileNameWithoutExtension(assetPath);
            var replacePath = assetPath;
            var removeDirectory = "Assets/Resources/";
            if (assetPath.Contains(removeDirectory))
            {
                var ext = Path.GetExtension(assetPath);
                replacePath = assetPath.Replace(removeDirectory, "");
                replacePath = replacePath.Replace(ext, "");
            }

            sb.AppendLine("        { " + fileName + ", \"" + replacePath + "\" },");
        }
        sb.AppendLine("    };");
        sb.AppendLine("");
        sb.AppendLine("    public static string GetPrefabPath(string tag)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (PrefabPaths.TryGetValue(tag, out var path))");
        sb.AppendLine("        {");
        sb.AppendLine("             return path;");
        sb.AppendLine("         }");
        sb.AppendLine("         return string.Empty;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string CreateDataKeyContents()
    {
        var assetGuids = AssetDatabase.FindAssets("t:ScriptableObject", new string[] { "Assets/Data" });
        string[] assetPathList = Array.ConvertAll<string, string>(assetGuids, AssetDatabase.GUIDToAssetPath);
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("");
        sb.AppendLine("public static class DataKeys");
        sb.AppendLine("{");

        // const string 생성 (접두사 포함)
        foreach (var assetPath in assetPathList)
        {
            var fileName = Path.GetFileNameWithoutExtension(assetPath);
            sb.AppendLine($"    public const string {fileName} = \"{fileName}\";");
        }

        sb.AppendLine("");
        sb.AppendLine("    public static Dictionary<string, string> DataPaths = new Dictionary<string, string>()");
        sb.AppendLine("    {");

        // Dictionary 생성 (접두사 포함된 키, 원본 경로)
        foreach (var assetPath in assetPathList)
        {
            var fileName = Path.GetFileNameWithoutExtension(assetPath);
            var keyName = GetDataKeyName(assetPath, fileName);
            sb.AppendLine("        { " + fileName + ", \"" + assetPath + "\" },");

        }

        sb.AppendLine("    };");
        sb.AppendLine("");
        sb.AppendLine("    public static string GetDataPath(string tag)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (DataPaths.TryGetValue(tag, out var path))");
        sb.AppendLine("        {");
        sb.AppendLine("            return path;");
        sb.AppendLine("        }");
        sb.AppendLine("        return string.Empty;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string CreateAudioKeyContents()
    {
        // Assets/Audio 하위의 AudioClip + AudioMixer 스캔 (폴더 없으면 Assets 전체)
        var searchFolders = AssetDatabase.IsValidFolder("Assets/Audio")
            ? new string[] { "Assets/Audio" }
            : new string[] { "Assets" };

        var assetGuids = AssetDatabase.FindAssets("t:AudioClip t:AudioMixer", searchFolders);
        string[] assetPathList = Array.ConvertAll<string, string>(assetGuids, AssetDatabase.GUIDToAssetPath);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("using System;");
        sb.AppendLine("using System.Collections.Generic;");
        sb.AppendLine("");
        sb.AppendLine("public static class AudioKeys");
        sb.AppendLine("{");

        // const string 생성 (파일명 그대로)
        foreach (var assetPath in assetPathList)
        {
            var fileName = Path.GetFileNameWithoutExtension(assetPath);
            sb.AppendLine($"    public const string {fileName} = \"{fileName}\";");
        }

        sb.AppendLine("");
        sb.AppendLine("    public static Dictionary<string, string> AudioPaths = new Dictionary<string, string>()");
        sb.AppendLine("    {");

        // Dictionary 생성 (파일명 키, 원본 경로)
        foreach (var assetPath in assetPathList)
        {
            var fileName = Path.GetFileNameWithoutExtension(assetPath);
            sb.AppendLine("        { " + fileName + ", \"" + assetPath + "\" },");
        }

        sb.AppendLine("    };");
        sb.AppendLine("");
        sb.AppendLine("    public static string GetAudioPath(string tag)");
        sb.AppendLine("    {");
        sb.AppendLine("        if (AudioPaths.TryGetValue(tag, out var path))");
        sb.AppendLine("        {");
        sb.AppendLine("            return path;");
        sb.AppendLine("        }");
        sb.AppendLine("        return string.Empty;");
        sb.AppendLine("    }");
        sb.AppendLine("}");
        return sb.ToString();
    }

    private static string GetDataKeyName(string assetPath, string fileName)
    {
        if (assetPath.Contains("/Ingredient/"))
            return "Ingredient_" + fileName;
        else if (assetPath.Contains("/Recipe/"))
            return "Recipe_" + fileName;
        else
            return fileName; // Stage 등 나머지는 접두사 없이
    }
}
