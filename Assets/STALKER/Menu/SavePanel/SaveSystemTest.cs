using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public static class SaveSystemTest
{
    public static string SaveDirectory
    {
        get
        {
            // Путь рядом с папкой Assets в редакторе или с EXE в билде
#if UNITY_EDITOR
            string path = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "SavedGames");
#else
            string path = Path.Combine(Application.dataPath, "SavedGames");
#endif
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            return path;
        }
    }

    public static List<string> GetSaveFiles()
    {
        List<string> saves = new List<string>();
        try
        {
            foreach (string file in Directory.GetFiles(SaveDirectory, "*.json"))
            {
                saves.Add(Path.GetFileNameWithoutExtension(file));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error reading saves: {e.Message}");
        }
        return saves;
    }

    public static void SaveGame(string saveName, object data)
    {
        string path = Path.Combine(SaveDirectory, $"{saveName}.json");
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    public static bool SaveExists(string saveName)
    {
        string path = Path.Combine(SaveDirectory, $"{saveName}.json");
        return File.Exists(path);
    }

    public static void DeleteSave(string saveName)
    {
        string path = Path.Combine(SaveDirectory, $"{saveName}.json");
        if (File.Exists(path)) File.Delete(path);
    }

}
