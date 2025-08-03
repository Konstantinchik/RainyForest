using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class GlobalPostProcessManager : MonoBehaviour
{
    public static GlobalPostProcessManager Instance { get; private set; }

    [Header("Профили постпроцессинга")]
    [SerializeField] private PostProcessProfile mainCameraProfile;
    [SerializeField] private PostProcessProfile waterProfile;

    private ColorGrading colorGrading;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (mainCameraProfile == null)
        {
            Debug.LogError("Main Camera Profile не назначен в инспекторе!");
            return;
        }

        // Применим настройки из основного профиля (Main Camera Profile)
        if (!mainCameraProfile.TryGetSettings(out colorGrading))
        {
            colorGrading = mainCameraProfile.AddSettings<ColorGrading>();
        }

        colorGrading.enabled.Override(true);
    }

    public void ApplyColorSettings(float gamma, float contrast, float brightness)
    {
        if (colorGrading == null)
        {
            Debug.LogWarning("ColorGrading не найден в профиле!");
            return;
        }

        colorGrading.gamma.Override(new Vector4(1f, 1f, 1f, gamma));
        colorGrading.contrast.Override(contrast);
        colorGrading.brightness.Override(brightness);

        Debug.Log($"[GlobalPostProcess] Gamma: {gamma}, Contrast: {contrast}, Brightness: {brightness}");
    }

    public void SwitchToWaterProfile()
    {
        if (waterProfile == null)
        {
            Debug.LogWarning("Water Profile не назначен!");
            return;
        }

        if (!waterProfile.TryGetSettings(out colorGrading))
        {
            colorGrading = waterProfile.AddSettings<ColorGrading>();
        }

        colorGrading.enabled.Override(true);
        Debug.Log("Switched to Water Profile.");
    }

    public void SwitchToMainCameraProfile()
    {
        if (mainCameraProfile == null)
        {
            Debug.LogWarning("Main Camera Profile не назначен!");
            return;
        }

        if (!mainCameraProfile.TryGetSettings(out colorGrading))
        {
            colorGrading = mainCameraProfile.AddSettings<ColorGrading>();
        }

        colorGrading.enabled.Override(true);
        Debug.Log("Switched to Main Camera Profile.");
    }
}
