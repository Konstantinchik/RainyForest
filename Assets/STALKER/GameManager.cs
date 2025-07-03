using DarkTreeFPS;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Центральный менеджер игры, управляющий состояниями, сценами и событиями
/// Запускается в сцене Intro раньше всех
/// </summary>
public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    #endregion

    #region Game State
    public enum GameState
    {
        Intro,                  // Вступительная сцена
        MainMenu,               // Главное меню (persistent сцена)
        Gameplay,               // Идет геймплей
        GamePaused,             // Игра на паузе (через кнопку паузы)
        InGameMenuAutoPaused,   // Меню в игре (автопауза)
        InGameMenuManualPaused  // Меню в игре (ручная пауза)
    }

    private GameState _currentState = GameState.Intro;
    public GameState CurrentState
    {
        get => _currentState;
        private set
        {
            if (_currentState != value)
            {
                var previousState = _currentState;
                _currentState = value;
                OnGameStateChanged?.Invoke(previousState, _currentState);
            }
        }
    }
    #endregion

    #region Events
    // Основные события игры
    public event Action<GameState, GameState> OnGameStateChanged; // Старое и новое состояние
    public static event Action OnGameStarted;                     // При переходе в Gameplay
    public static event Action OnGamePaused;                      // При любой паузе
    public static event Action OnGameResumed;                     // При снятии паузы
    public static event Action OnReturnedToMenu;                  // Возврат в главное меню
    public static event Action<GameObject> OnPlayerSpawned;       // При спавне игрока
    #endregion

    #region Scene Management
    private const string MainMenuSceneName = "MainMenu_P";
    public List<string> LoadedGameScenes { get; private set; } = new List<string>();
    private GameObject _currentPlayer;
    #endregion

    #region Unity Callbacks
    private void Awake()
    {
        InitializeSingleton();
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            Instance = null;
        }
    }

    private void Update()
    {
        HandleEscapeInput();
        HandleDebugInput();
    }
    #endregion

    #region Initialization
    private void InitializeSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region State Management
    /// <summary>
    /// Переключает состояние паузы
    /// </summary>
    public void TogglePause()
    {
        if (CurrentState == GameState.MainMenu) return;

        bool shouldPause = CurrentState switch
        {
            GameState.Gameplay => true,
            _ => false
        };

        SetPaused(shouldPause);
    }

    /// <summary>
    /// Устанавливает состояние паузы
    /// </summary>
    public void SetPaused(bool paused)
    {
        if (CurrentState == GameState.MainMenu) return;

        if (paused)
        {
            CurrentState = CurrentState == GameState.GamePaused ?
                GameState.InGameMenuManualPaused :
                GameState.InGameMenuAutoPaused;

            Time.timeScale = 0f;
            OnGamePaused?.Invoke();
        }
        else
        {
            CurrentState = GameState.Gameplay;
            Time.timeScale = 1f;
            OnGameResumed?.Invoke();
        }
    }

    /// <summary>
    /// Переключает между игрой и главным меню
    /// </summary>
    public void ToggleMainMenu()
    {
        if (CurrentState == GameState.MainMenu)
            StartGame();
        else
            ReturnToMenu();
    }

    /// <summary>
    /// Запускает игру (из главного меню)
    /// </summary>
    public void StartGame()
    {
        UnloadAllGameScenes();
        // Здесь должна быть логика загрузки начальной игровой сцены
        CurrentState = GameState.Gameplay;
        OnGameStarted?.Invoke();
    }

    /// <summary>
    /// Возвращает в главное меню
    /// </summary>
    public void ReturnToMenu()
    {
        UnloadAllGameScenes();
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(MainMenuSceneName));
        CurrentState = GameState.MainMenu;
        OnReturnedToMenu?.Invoke();
    }
    #endregion

    #region Scene Management
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != MainMenuSceneName && !LoadedGameScenes.Contains(scene.name))
        {
            LoadedGameScenes.Add(scene.name);
        }
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (LoadedGameScenes.Contains(scene.name))
        {
            LoadedGameScenes.Remove(scene.name);
        }
    }

    /// <summary>
    /// Выгружает все игровые сцены
    /// </summary>
    public void UnloadAllGameScenes()
    {
        foreach (var sceneName in new List<string>(LoadedGameScenes))
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }
        LoadedGameScenes.Clear();
    }

    /// <summary>
    /// Добавляет игровую сцену
    /// </summary>
    public void AddGameScene(string sceneName)
    {
        if (!LoadedGameScenes.Contains(sceneName))
        {
            LoadedGameScenes.Add(sceneName);
        }
    }

    /// <summary>
    /// Удаляет игровую сцену
    /// </summary>
    public void RemoveGameScene(string sceneName)
    {
        if (LoadedGameScenes.Contains(sceneName))
        {
            LoadedGameScenes.Remove(sceneName);
        }
    }
    #endregion

    #region Player Management
    public static void NotifyPlayerSpawned(GameObject player)
    {
        Instance.HandlePlayerSpawned(player);
        OnPlayerSpawned?.Invoke(player);
    }

    private void HandlePlayerSpawned(GameObject player)
    {
        _currentPlayer = player;
        Debug.Log($"[GameManager] Player spawned: {player.name}");

        // Инициализация компонентов игрока
        var stats = player.GetComponent<PlayerStats>();
        var cam = player.GetComponentInChildren<Camera>();
    }
    #endregion

    #region Input Handling
    private void HandleEscapeInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            switch (CurrentState)
            {
                case GameState.Gameplay:
                    SetPaused(true);
                    break;
                case GameState.InGameMenuAutoPaused:
                case GameState.InGameMenuManualPaused:
                    SetPaused(false);
                    break;
                case GameState.MainMenu:
                    // Логика выхода из игры
                    break;
            }
        }
    }

    private void HandleDebugInput()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.T))
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
#endif
    }
    #endregion

    #region Load Game Scene
    public void LoadGameScene(string sceneName)
    {
        StartCoroutine(LoadSceneAdditive(sceneName));
    }

    private IEnumerator LoadSceneAdditive(string sceneName)
    {
        // Выгружаем предыдущие игровые сцены
        UnloadAllGameScenes();

        // Загружаем новую сцену
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            if (asyncLoad.progress >= 0.9f)
            {
                asyncLoad.allowSceneActivation = true;
            }
            yield return null;
        }

        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(loadedScene);
        AddGameScene(sceneName);
        CurrentState = GameState.Gameplay;
    }
    #endregion
}



public static class SaveSystem
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
}