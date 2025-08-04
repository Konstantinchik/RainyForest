using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using static KeybindManager;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;


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

    [SerializeField] private Button applyButton;
    [SerializeField] private Button resetButton;

    //[SerializeField] private TMP_Dropdown textureQualityDropdown;

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

        //SetDefaultSettings();
        //SaveDefaultSettings();

        // SetupButtons();

        LoadSettings();

        // Инициализация выпадающих списков
        InitializeDropdowns();
    }

    #endregion

    // Сейчас 
    private void SetupButtons()
    {
        applyButton.onClick.AddListener(ApplySettings);
        resetButton.onClick.AddListener(ResetToDefault);
    }

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

        // Автовыбор текущего API
        string api = SystemInfo.graphicsDeviceType.ToString();
        renderDropdown.value = api.Contains("Direct3D12") ? 1 : 0;

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
        public int renderType;
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
        // Разрешение экрана
        var resolution = _resolutions[currentSettings.resolutionIndex];
        Screen.SetResolution(
            Screen.resolutions[currentSettings.resolutionIndex].width,
            Screen.resolutions[currentSettings.resolutionIndex].height,
            currentSettings.isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed,
            Screen.resolutions[currentSettings.resolutionIndex].refreshRateRatio
        );
        // Здесь можно добавить применение gamma, contrast и brightness
        // Применяем настройки цветокоррекции
        ApplyColorGradingSettings(
            currentSettings.gamma,
            currentSettings.contrast,
            currentSettings.brightness
        );

        SaveSettings();
        Debug.Log("Video settings applied!");
    }

    private void ApplyColorGradingSettings(float gamma, float contrast, float brightness)
    {
        // Получаем PostProcessVolume из сцены
        PostProcessVolume volume = FindFirstObjectByType<PostProcessVolume>();
        if (volume == null)
        {
            Debug.LogWarning("PostProcessVolume not found in scene!");
            return;
        }

        // Получаем или добавляем компонент ColorGrading
        if (!volume.profile.TryGetSettings(out ColorGrading colorGrading))
        {
            colorGrading = volume.profile.AddSettings<ColorGrading>();
        }

        // Применяем настройки
        colorGrading.enabled.Override(true);
        colorGrading.gamma.Override(new Vector4(1f, 1f, 1f, gamma));
        colorGrading.contrast.Override(contrast);
        colorGrading.brightness.Override(brightness);

        Debug.Log($"Applied color settings - Gamma: {gamma}, Contrast: {contrast}, Brightness: {brightness}");
    }

    public void ResetToDefault()
    {
        /*
        currentSettings = new VideoSettings
        {
            renderType = defaultSettings.renderType,
            qualityLevel = defaultSettings.qualityLevel,
            resolutionIndex = defaultSettings.resolutionIndex,
            isFullscreen = defaultSettings.isFullscreen,
            gamma = defaultSettings.gamma,
            contrast = defaultSettings.contrast,
            brightness = defaultSettings.brightness
        };*/
        currentSettings = new VideoSettings
        {
            renderType = 0,
            qualityLevel = QualitySettings.GetQualityLevel(),
            resolutionIndex = GetCurrentResolutionIndex(),
            isFullscreen = true,
            gamma = 0.5f,
            contrast = 0.5f,
            brightness = 0.5f
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

    [Obsolete]
    private int GetCurrentResolutionIndex()
    {
        Resolution current = Screen.currentResolution;
        for (int i = 0; i < _resolutions.Length; i++)
        {
            if (_resolutions[i].width == current.width &&
                _resolutions[i].height == current.height &&
                _resolutions[i].refreshRate == current.refreshRate)
            {
                return i;
            }
        }
        return 0; // Возвращаем 0, если разрешение не найдено
    }
    private void SetDefaultSettings()
    {

        defaultSettings.renderType = 0;
        defaultSettings.qualityLevel = QualitySettings.GetQualityLevel();
        defaultSettings.resolutionIndex = GetCurrentResolutionIndex();
        defaultSettings.isFullscreen = true;
        defaultSettings.gamma = 0.5f;
        defaultSettings.contrast = 0.5f;
        defaultSettings.brightness = 0.5f;
    }

    private void SaveDefaultSettings()
    {
        currentSettings = new VideoSettings
        {
            renderType = defaultSettings.renderType,
            qualityLevel = defaultSettings.qualityLevel,
            //resolutionIndex = defaultSettings.resolutionIndex, - у всех свой
            isFullscreen = defaultSettings.isFullscreen,
            gamma = defaultSettings.gamma,
            contrast = defaultSettings.contrast,
            brightness = defaultSettings.brightness
        };

        string json = JsonUtility.ToJson(currentSettings, true);
        File.WriteAllText(defaultSavePath, json);

        UpdateUI();
    }

    public void LoadSettings()
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
                renderType = defaultSettings.renderType,
                qualityLevel = QualitySettings.GetQualityLevel(),
                resolutionIndex = Array.FindIndex(Screen.resolutions,
                    r => r.width == Screen.width && r.height == Screen.height),
                isFullscreen = Screen.fullScreen,
                gamma = 0.5f,
                contrast = 0.5f,
                brightness = 0.5f
            };
        }

        UpdateUI();
    }

    private void UpdateUI()
    {
        renderDropdown.value = currentSettings.renderType;
        qualityDropdown.value = currentSettings.qualityLevel;
        resolutionDropdown.value = currentSettings.resolutionIndex;
        fullScreenToggle.isOn = currentSettings.isFullscreen;
        gammaSlider.value = currentSettings.gamma;
        contrastSlider.value = currentSettings.contrast;
        brightnessSlider.value = currentSettings.brightness;
    }
    #endregion




    #region UI Event Handlers
    public void OnRenderTypeChanged(int index)
    {
        currentSettings.renderType = index;
        Debug.Log("currentSettings.renderType = " + index);
    }

    public void OnQualityChanged(int index)
    {
        currentSettings.qualityLevel = index;
        Debug.Log("currentSettings.qualityLevel = " + index);
    }

    public void OnResolutionChanged(int index)
    {
        currentSettings.resolutionIndex = index;
        Debug.Log("currentSettings.resolutionIndex = " + index);
    }

    public void OnFullscreenChanged(bool value)
    {
        currentSettings.isFullscreen = value;
        Debug.Log("currentSettings.isFullscreen = " + value);
    }

    public void OnGammaChanged(float value)
    {
        currentSettings.gamma = value;
        Debug.Log("currentSettings.gamma = " + value);
    }

    public void OnContrastChanged(float value)
    {
        currentSettings.contrast = value;
        Debug.Log("currentSettings.contrast = " + value);
    }

    public void OnBrightnessChanged(float value)
    {
        currentSettings.brightness = value;
        Debug.Log("currentSettings.brightness = " + value);
    }
    #endregion

    #region [SET DIRECTX11 OR DIECTX12]
    // Только для Windows Standalone
    void SetDirectXVersion(bool useDX12)
    {
#if UNITY_EDITOR
        GraphicsDeviceType[] apis = useDX12 ?
            new[] { GraphicsDeviceType.Direct3D12 } :
            new[] { GraphicsDeviceType.Direct3D11 };

        PlayerSettings.SetGraphicsAPIs(BuildTarget.StandaloneWindows, apis);
        Debug.Log("API changed - requires restart!");
#else
    Debug.LogWarning("Changing Graphics API at runtime is only supported in the Unity Editor.");
#endif
    }
    #endregion
}