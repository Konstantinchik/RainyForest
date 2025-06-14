using DarkTreeFPS;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    public static MainMenuController Instance { get; private set; }

    [SerializeField] private GameObject _mainMenuCamera;

    /* ---------- UI ---------- */
    [Header("������")]
    public Button newGameButton;
    public Button resumeGameButton;
    public Button loadGameButton;
    public Button optionsButton;
    public Button exitButton;

    [Header("������")]
    public GameObject optionsPanel;          // ������� ��������
    public GameObject pauseMenuPanel;        // ���� ���� ��������� ������ �����

    /* ---------- ��������� ---------- */
    [Header("���������")]
    public string newGameSceneName = "GameScene"; // ��� ����� ��� "����� ����"
    

    /* ---------- ���������� ---------- */
    public bool IsActive => isMenuActive;

    private bool isMenuActive = true;
    private string currentGameScene;

    #region Unity Lifecycle
    private void Awake()
    {
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {

        // PlayerPrefs.DeleteAll();

        // ����������� ������ �� ������
        newGameButton.onClick.AddListener(StartNewGame);
        resumeGameButton.onClick.AddListener(ResumeGame);
        loadGameButton.onClick.AddListener(LoadGame);
        optionsButton.onClick.AddListener(ToggleOptions);
        exitButton.onClick.AddListener(ExitGame);

        resumeGameButton.interactable = SaveSystem.HasSave();

    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            Instance = null;
        }
    }
    #endregion

    /* ---------- Scene Events ---------- */
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        /* ���� ��� �� MainMenu_P, ���������� ��� �������� �������� */
        if (scene.name != "MainMenu_P")
        {
            currentGameScene = scene.name;
            /* ���� ������ ���� ������, ���� �� ����� � ��������� */
            SetMenuActive(false);
        }
    }


    /* ---------- ���� On/Off ---------- */
    private void SetMenuActive(bool active)
    {
        isMenuActive = active;

        /* ���������� / ������ ��� �������� UI-�������� */
        foreach (Transform child in transform)
        {
            if (child.gameObject != gameObject)
                child.gameObject.SetActive(active);
        }

        // ���������� ������� ������� ����
        if (_mainMenuCamera != null)
            _mainMenuCamera.SetActive(active);

        /* ���������� ������� � �������� */
        if (active)
        {
            SoundController.Instance.PlayMenuMusic();
            Time.timeScale = 0f;
        }
        else
        {
            SoundController.Instance.StopMenuMusic();
            Time.timeScale = 1f;
            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        }
    }

    /* ---------- API ��� Pause-���� ---------- */
    public void ReturnToMainMenu()          // ���������� ������� "Main Menu"
    {
        /* ��������� ����� */
        Time.timeScale = 0f;
        /* ��������� ������� ������� �����, ���� ��� ���� */
        if (!string.IsNullOrEmpty(currentGameScene))
            StartCoroutine(UnloadCurrentGameScene());

        /* ���������� ������� ���� */
        SetMenuActive(true);
    }

    private IEnumerator UnloadCurrentGameScene()
    {
        AsyncOperation op = SceneManager.UnloadSceneAsync(currentGameScene);
        while (!op.isDone) yield return null;
        currentGameScene = null;
    }

    /* ---------- ������ �������� ���� ---------- */
    private void StartNewGame()
    {
        StartCoroutine(LoadSceneAdditive(newGameSceneName));
    }

    private void ResumeGame()
    {
        if (SaveSystem.HasSave())
        {
            /* ��� ����� ���� ���� ������ �������� ��������� */
            StartCoroutine(LoadSceneAdditive(newGameSceneName));
        }
    }

    private void LoadGame()
    {
        /* �������� ��� ����� ����� � �.�. */
        StartCoroutine(LoadSceneAdditive(newGameSceneName));
    }

    private void ToggleOptions()
    {
        if (optionsPanel != null) optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

    private void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    /* ---------- Additive Loader ---------- */
    private IEnumerator LoadSceneAdditive(string sceneName)
    {
        /* ��������� ���������� �������, ���� ���� */
        if (!string.IsNullOrEmpty(currentGameScene))
        {
            AsyncOperation unload = SceneManager.UnloadSceneAsync(currentGameScene);
            while (!unload.isDone) yield return null;
        }

        /* ��������� ����� */
        AsyncOperation load = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!load.isDone) yield return null;

        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(loadedScene);
        currentGameScene = sceneName;

        /* ����� �������� � ������� ���� � �������� ���� */
        SetMenuActive(false);
    }
}


// ����� - ��������         SAVE SYSTEM 
public static class SaveSystem
{
    private const string SAVE_KEY = "GameSave";

    // ���������, ���� �� ����������
    public static bool HasSave()
    {
        return PlayerPrefs.HasKey(SAVE_KEY);
    }

    // ��������� ������
    public static void LoadGame()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY);
        // ������������� json � ������� ������
    }
}