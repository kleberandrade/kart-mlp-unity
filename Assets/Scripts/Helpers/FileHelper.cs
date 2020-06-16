using UnityEditor;
using UnityEngine;

public class FileHelper
{
    public static string GetResourcePath(string filename)
    {
        return $"{Application.dataPath}/Resources/{filename}";
    }

    public static TextAsset LoadTextResource(string filename)
    {
        return Resources.Load<TextAsset>(filename.Split('.')[0]);
    }

    public static void SaveFile(string filename, string data)
    {
        System.IO.File.WriteAllText(GetResourcePath(filename), data);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
