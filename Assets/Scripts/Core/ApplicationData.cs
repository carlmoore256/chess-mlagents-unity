using System.IO;
using UnityEngine;
using System.Linq;
using System;

public class ApplicationData
{

    public static void SaveText(string text, string filename)
    {
        var path = Path.Combine(Application.persistentDataPath, filename);
        File.WriteAllText(path, text);
    }

    public static void SaveText(string text, string folder, string filename)
    {
        if (!Directory.Exists(Path.Combine(Application.persistentDataPath, folder)))
        {
            Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, folder));
        }
        var path = Path.Combine(Application.persistentDataPath, folder, filename);
        Debug.Log($"[ApplicationData] Saving to {path}");
        File.WriteAllText(path, text);
    }

    public static T LoadFromText<T>(string filename, Func<string, T> parseFn)
    {
        var path = Path.Combine(Application.persistentDataPath, filename);
        var text = File.ReadAllText(path);
        return parseFn(text);
    }

    public static T LoadFromText<T>(string folder, string filename, Func<string, T> parseFn)
    {
        var path = Path.Combine(Application.persistentDataPath, folder, filename);
        var text = File.ReadAllText(path);
        return parseFn(text);
    }

    public static string LoadText(string filename)
    {
        var path = Path.Combine(Application.persistentDataPath, filename);
        return File.ReadAllText(path);
    }

    public static string LoadText(string folder, string filename)
    {
        var path = Path.Combine(Application.persistentDataPath, folder, filename);
        return File.ReadAllText(path);
    }

    public static string[] GetFiles(string folder, bool removeExtension = false)
    {
        var path = Path.Combine(Application.persistentDataPath, folder);
        if (!Directory.Exists(path))
        {
            return null;
        }
        var files = Directory.GetFiles(path);
        if (removeExtension) files = files.Select(f => Path.GetFileNameWithoutExtension(f)).ToArray();
        return files;
    }
}