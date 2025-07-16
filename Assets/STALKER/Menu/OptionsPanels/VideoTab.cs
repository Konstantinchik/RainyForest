using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static KeybindManager;
//using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;
using System;

public class VideoTab : MonoBehaviour
{
    [Header("References")]
    [SerializeField] OptionsPanel _optionsPanel;
    [SerializeField] TMP_Dropdown renderDropdown;
    [SerializeField] TMP_Dropdown qualityDropdown;
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] Toggle fullScreenToggle;
    [SerializeField] Slider gammaSlider;
    [SerializeField] Slider contrastSlider;
    [SerializeField] Slider brightnessSlider;

    [SerializeField] private TMP_Dropdown textureQualityDropdown;

    [Header("Settings")]
    [SerializeField] private VideoSettings defaultSettings;
    private VideoSettings currentSettings;

    private Resolution[] _resolutions;

    #region [SELECT TAB]
    public void ShowVideoTab()
    {
        _optionsPanel.ShowVideoTab();
    }

    public void ShowSoundTab()
    {
        _optionsPanel.ShowSoundTab();
    }

    public void ShowGameTab()
    {
        _optionsPanel.ShowGameTab();
    }

    public void ShowControlTab()
    {
        _optionsPanel.ShowControlTab();
    }
    #endregion

    #region [Set Save Path]
    private string defaultSavePath
    {
        get
        {
            // поднимаемся на один уровень вверх 
            string folderPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Default");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            return Path.Combine(folderPath, "videosettings_default.json");
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
            return Path.Combine(folderPath, "videosettings.json");
        }
    }
    #endregion

    #region Unity Events
    private void Awake()
    {
        _resolutions = Screen.resolutions;

        // Инициализация выпадающих списков
        InitializeDropdowns();
    }

    #endregion

    #region [Dropdown Initialization]
    private void InitializeDropdowns()
    {
        // Render Settings
        renderDropdown.ClearOptions();
        renderDropdown.AddOptions(new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData("DirectX 11"),
            new TMP_Dropdown.OptionData("DirectX 12")
        });

        // Quality Settings
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<TMP_Dropdown.OptionData>
        {
            new TMP_Dropdown.OptionData("минимальные"),
            new TMP_Dropdown.OptionData("низкие"),
            new TMP_Dropdown.OptionData("средние"),
            new TMP_Dropdown.OptionData("высокие"),
            new TMP_Dropdown.OptionData("максимальные")
        });
    
        // Resolution Settings
        resolutionDropdown.ClearOptions();
        var resolutionOptions = _resolutions
            .Select(r => $"{r.width}x{r.height} @ {r.refreshRateRatio}Hz")
            .ToList();
        resolutionDropdown.AddOptions(resolutionOptions);
    }
    #endregion

    #region Settings Management
    [System.Serializable]
    private class VideoSettings
    {
        public int qualityLevel;
        public int resolutionIndex;
        public bool isFullscreen;
        public float gamma;
        public float contrast;
        public float brightness;
    }

    public void ApplySettings()
    {
        // Применяем текущие настройки
        QualitySettings.SetQualityLevel(currentSettings.qualityLevel);
        Screen.SetResolution(
            Screen.resolutions[currentSettings.resolutionIndex].width,
            Screen.resolutions[currentSettings.resolutionIndex].height,
            currentSettings.isFullscreen
        );
        // Здесь можно добавить применение gamma, contrast и brightness

        SaveSettings();
        Debug.Log("Video settings applied!");
    }

    public void ResetToDefault()
    {
        currentSettings = new VideoSettings
        {
            qualityLevel = defaultSettings.qualityLevel,
            resolutionIndex = defaultSettings.resolutionIndex,
            isFullscreen = defaultSettings.isFullscreen,
            gamma = defaultSettings.gamma,
            contrast = defaultSettings.contrast,
            brightness = defaultSettings.brightness
        };

        UpdateUI();
        ApplySettings();
        Debug.Log("Video settings reset to default!");
    }

    private void SaveSettings()
    {
        string json = JsonUtility.ToJson(currentSettings, true);
        File.WriteAllText(savePath, json);
    }

    private void LoadSettings()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            currentSettings = JsonUtility.FromJson<VideoSettings>(json);
        }
        else
        {
            currentSettings = new VideoSettings
            {
                qualityLevel = QualitySettings.GetQualityLevel(),
                resolutionIndex = Array.FindIndex(Screen.resolutions,
                    r => r.width == Screen.width && r.height == Screen.height),
                isFullscreen = Screen.fullScreen,
                gamma = 1.0f,
                contrast = 1.0f,
                brightness = 1.0f
            };
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        qualityDropdown.value = currentSettings.qualityLevel;
        resolutionDropdown.value = currentSettings.resolutionIndex;
        fullScreenToggle.isOn = currentSettings.isFullscreen;
        gammaSlider.value = currentSettings.gamma;
        contrastSlider.value = currentSettings.contrast;
        brightnessSlider.value = currentSettings.brightness;
    }
    #endregion





    #region UI Event Handlers
    public void OnQualityChanged(int index)
    {
        currentSettings.qualityLevel = index;
    }

    public void OnResolutionChanged(int index)
    {
        currentSettings.resolutionIndex = index;
    }

    public void OnFullscreenChanged(bool value)
    {
        currentSettings.isFullscreen = value;
    }

    public void OnGammaChanged(float value)
    {
        currentSettings.gamma = value;
    }

    public void OnContrastChanged(float value)
    {
        currentSettings.contrast = value;
    }

    public void OnBrightnessChanged(float value)
    {
        currentSettings.brightness = value;
    }
    #endregion
}
