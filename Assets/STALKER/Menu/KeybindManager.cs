using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ActionBinding
{
    public string actionName;       // Название действия (например, "Прыжок")
    public TMP_Text buttonText;     // Отображение в UI
    public Button uiButton;         // Кнопка для назначения
    public KeyCode key;             // Назначенная клавиша
}

public class KeybindManager : MonoBehaviour
{
    [Serializable]
    public class KeyBindingData
    {
        public List<string> actionNames = new();
        public List<KeyCode> keys = new();
    }

    public List<ActionBinding> bindings; // Жестко по порядку

    private int listeningIndex = -1;     // Текущий индекс действия, ожидающего ввода
    private Dictionary<KeyCode, int> keyToActionIndex = new(); // Клавиша → индекс действия

    private string savePath => Path.Combine(Application.persistentDataPath, "keybindings.json");


    void Start()
    {
        LoadBindings();

        for (int i = 0; i < bindings.Count; i++)
        {
            int index = i;
            bindings[i].uiButton.onClick.AddListener(() =>
            {
                StartListening(index);
            });

            // Инициализация словаря
            keyToActionIndex[bindings[i].key] = i;
            bindings[i].buttonText.text = GetKeyLabel(bindings[i].key);
        }
    }

    void Update()
    {
        if (listeningIndex == -1)
            return;

        if (Input.anyKeyDown)
        {
            foreach (KeyCode kc in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(kc))
                {
                    AssignKeyToIndex(listeningIndex, kc);
                    listeningIndex = -1;
                    //SaveBindigs();
                    break;
                }
            }
        }
    }

    void StartListening(int index)
    {
        listeningIndex = index;
        bindings[index].buttonText.text = "<...>";
    }

    void AssignKeyToIndex(int index, KeyCode newKey)
    {
        // Если клавиша уже использовалась — очистим старую
        if (keyToActionIndex.TryGetValue(newKey, out int oldIndex))
        {
            bindings[oldIndex].key = KeyCode.None;
            bindings[oldIndex].buttonText.text = "--";
            keyToActionIndex.Remove(newKey);
        }

        // Удалим старое назначение клавиши, если было
        if (bindings[index].key != KeyCode.None)
        {
            keyToActionIndex.Remove(bindings[index].key);
        }

        // Назначим новую
        bindings[index].key = newKey;
        bindings[index].buttonText.text = GetKeyLabel(newKey);
        keyToActionIndex[newKey] = index;
    }

    #region SAVE and LOAD
    public void SaveBindings()
    {
        KeyBindingData data = new();
        foreach (var b in bindings)
        {
            data.actionNames.Add(b.actionName);
            data.keys.Add(b.key);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"Key bindings saved to: {savePath}");
    }

    public void LoadBindings()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("No saved bindings found, using defaults.");
            return;
        }

        string json = File.ReadAllText(savePath);
        KeyBindingData data = JsonUtility.FromJson<KeyBindingData>(json);
        if (data.actionNames.Count != bindings.Count)
        {
            Debug.LogWarning("Saved bindings do not match current bindings list.");
            return;
        }

        for (int i = 0; i < bindings.Count; i++)
        {
            bindings[i].key = data.keys[i];
        }
        Debug.Log("Key bindings loaded.");
    }

    #endregion

    string GetKeyLabel(KeyCode key)
    {
        if (key == KeyCode.None) return " --";
        if (key.ToString().ToLower().StartsWith("mouse")) return key.ToString().ToLower();
        if (key == KeyCode.Space) return " Space";
        if (key == KeyCode.LeftControl || key == KeyCode.RightControl) return " Ctrl";
        if (key == KeyCode.LeftShift || key == KeyCode.RightShift) return " Shift";
        if (key == KeyCode.BackQuote) return " ~";
        if (key == KeyCode.Return) return " Enter";
        if (key == KeyCode.Tab) return " Tab";
        if (key == KeyCode.Escape) return " Esc";
        if (key == KeyCode.Backspace) return " Backspace";
        if (key == KeyCode.Print) return " PrtScrn";
        if (key == KeyCode.PageUp) return " PageUp";
        if (key == KeyCode.PageDown) return " PageDown";
        return key.ToString().ToUpper();
    }

    public KeyCode GetKeyByIndex(int index)
    {
        return bindings[index].key;
    }

    public KeyCode GetKeyByAction(string actionName)
    {
        foreach (var b in bindings)
            if (b.actionName == actionName)
                return b.key;
        return KeyCode.None;
    }
}
