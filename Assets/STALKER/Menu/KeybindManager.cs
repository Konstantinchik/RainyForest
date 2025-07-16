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
    [Header("Дополнительные настройки")]
    public Slider mouseSensitivitySlider;
    public Toggle inverseMouseToggle;
    public Scrollbar scrollBar;

    [Header("Применить или отменить")]
    public Button defaultButton;
    public Button appluButton;
    public Button cancelButton;

    [Serializable]
    public class KeyBindingData
    {
        public List<string> actionNames = new();
        public List<KeyCode> keys = new();
    }

    public List<ActionBinding> bindings; // Жестко по порядку

    private int listeningIndex = -1;     // Текущий индекс действия, ожидающего ввода
    private Dictionary<KeyCode, int> keyToActionIndex = new(); // Клавиша → индекс действия

    #region [Set Save Path]
    private string defaultSavePath
    {
        get
        {
            // поднимаемся на один уровень вверх 
            string folderPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Default");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            return Path.Combine(folderPath, "keybindings_default.json");
        }
    }

    private string savePath
    {
        get
        {
            // поднимаемся на один уровень вверх 
            string folderPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Options");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            return Path.Combine(folderPath, "keybindings.json");
        }
    }
    #endregion

    void Start()
    {
        LoadBindings();
        //SaveDefaultBindings(); // - для разработчиков

        InitControlUI();        // Привязываем UI
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
                    SaveBindings();
                    break;
                }
            }
        }
    }

    private void ApplyDefaultExtraSettings()
    {
        if (mouseSensitivitySlider != null)
            mouseSensitivitySlider.value = 0.7f;

        if (inverseMouseToggle != null)
            inverseMouseToggle.isOn = false;

        if (scrollBar != null)
            scrollBar.value = 1f;
    }

    public void InitControlUI()
    {
        keyToActionIndex.Clear();

        for (int i = 0; i < bindings.Count; i++)
        {
            int index = i;

            // Удалим старые слушатели перед добавлением новых
            bindings[i].uiButton.onClick.RemoveAllListeners();

            // Назначим новый обработчик
            bindings[i].uiButton.onClick.AddListener(() =>
            {
                StartListening(index);
            });

            // Обновим отображение клавиши и словарь
            bindings[i].buttonText.text = GetKeyLabel(bindings[i].key);
            if (bindings[i].key != KeyCode.None)
                keyToActionIndex[bindings[i].key] = index;
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
            RefreshButtonLabel(oldIndex);
            keyToActionIndex.Remove(newKey);
        }

        // Удалим старое назначение клавиши, если было
        if (bindings[index].key != KeyCode.None)
        {
            keyToActionIndex.Remove(bindings[index].key);
        }

        // Назначим новую
        bindings[index].key = newKey;
        keyToActionIndex[newKey] = index;
        RefreshButtonLabel(index);
    }

    #region SAVE and LOAD DEFAULT Bindings
    
    // For DEVELOPERS ONLY
    public void SaveDefaultBindings()
    {
        KeyBindingData data = new();
        foreach (var b in bindings)
        {
            data.actionNames.Add(b.actionName);
            data.keys.Add(b.key);
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(defaultSavePath, json);
        Debug.Log($"Default key bindings saved to: {defaultSavePath}");
    }


    public void LoadDefaultBindings()
    {
        if (!File.Exists(defaultSavePath))
        {
            Debug.LogWarning($"Default key bindings file not found at: {defaultSavePath}");
            return;
        }

        string json = File.ReadAllText(defaultSavePath);
        KeyBindingData data = JsonUtility.FromJson<KeyBindingData>(json);

        if (data.actionNames.Count != bindings.Count)
        {
            Debug.LogWarning("Default bindings do not match current bindings list.");
            return;
        }

        keyToActionIndex.Clear();

        for (int i = 0; i < bindings.Count; i++)
        {
            bindings[i].key = data.keys[i];
            bindings[i].buttonText.text = GetKeyLabel(data.keys[i]);
            keyToActionIndex[data.keys[i]] = i;
        }

        Debug.Log("Default key bindings loaded.");
        InitControlUI();
        ApplyDefaultExtraSettings();
    }

    #endregion

    #region SAVE and LOAD Bindings

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
            LoadDefaultBindings();
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

        InitControlUI();
    }

    #endregion

    #region [Get Key]
    string GetKeyLabel(KeyCode key)
    {
        if (key == KeyCode.None) return " --";
        if (key.ToString().ToLower().StartsWith("mouse")) return key.ToString().ToLower();
        if (key == KeyCode.LeftArrow) return " Left";
        if (key == KeyCode.RightArrow) return " Right";
        if (key == KeyCode.UpArrow) return " Up";
        if (key == KeyCode.DownArrow) return " Down";
        if (key == KeyCode.Alpha0) return " 0";
        if (key == KeyCode.Alpha1) return " 1";
        if (key == KeyCode.Alpha2) return " 2";
        if (key == KeyCode.Alpha3) return " 3";
        if (key == KeyCode.Alpha4) return " 4";
        if (key == KeyCode.Alpha5) return " 5";
        if (key == KeyCode.Alpha6) return " 6";
        if (key == KeyCode.Alpha7) return " 7";
        if (key == KeyCode.Alpha8) return " 8";
        if (key == KeyCode.Alpha9) return " 9";
        if (key == KeyCode.Space) return " Space";
        if (key == KeyCode.LeftControl || key == KeyCode.RightControl) return " Ctrl";
        if (key == KeyCode.LeftShift || key == KeyCode.RightShift) return " Shift";
        if (key == KeyCode.LeftAlt || key == KeyCode.RightAlt) return " Alt";
        if (key == KeyCode.BackQuote) return " ~";
        if (key == KeyCode.Return) return " Enter";
        if (key == KeyCode.Tab) return " Tab";
        if (key == KeyCode.Escape) return " Esc";
        if (key == KeyCode.CapsLock) return " Caps";
        if (key == KeyCode.Backspace) return " Backspace";
        if (key == KeyCode.Print) return " PrtScrn";
        if (key == KeyCode.PageUp) return " PageUp";
        if (key == KeyCode.PageDown) return " PageDown";
        if (key == KeyCode.Home) return " Home";
        if (key == KeyCode.End) return " End";
        if (key == KeyCode.Insert) return " Ins";
        if (key == KeyCode.Delete) return " Del";
        if (key == KeyCode.Numlock) return " Num";
        if (key == KeyCode.ScrollLock) return " Scroll";
        if (key == KeyCode.LeftBracket) return " [";
        if (key == KeyCode.RightBracket) return " ]";

        return " " + key.ToString().ToUpper();
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
    #endregion

    void RefreshButtonLabel(int index)
    {
        bindings[index].buttonText.text = GetKeyLabel(bindings[index].key);
    }
}
