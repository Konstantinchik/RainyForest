using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine;

public static class SaveSystemTest
{
    private const string SAVE_KEY = "GameSave";
    private const string LEVEL_KEY = "LastLevel";

    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    public static void CreateNewSave()
    {
        PlayerPrefs.SetInt(SAVE_KEY, 1);
        PlayerPrefs.Save();
    }

    public static string GetLastLevel()
    {
        return PlayerPrefs.GetString(LEVEL_KEY, "");
    }

    public static void SaveLevel(string levelName)
    {
        PlayerPrefs.SetString(LEVEL_KEY, levelName);
        PlayerPrefs.Save();
    }

    internal static GameSaveData LoadGame(string saveName)
    {
        throw new NotImplementedException();
    }

    internal static void SaveGame(string saveName, GameSaveData data)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Новый раздел
    /// </summary>
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

    public static SaveMetaData LoadMetaData(string saveName)
    {
        var meta = new SaveMetaData
        {
            levelName = "Dark Valley",
            dateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            healthPercent = 72 // Пример: получи это из PlayerHealth
        };

        return meta;
    }
}

[System.Serializable]
public class SaveMetaData
{
    public string levelName;
    public string dateTime;
    public int healthPercent;
}

#region [GameSaveData public serializable class]
/// <summary>
/// Данные для сохранения. В будущем будут в отдельном файле
/// </summary>

[Serializable]
public class GameSaveData
{
    public string levelName;      // Имя текущей сцены
    public Vector3 playerPosition; // Позиция игрока
    public DateTime timestamp;    // Время сохранения
}
#endregion