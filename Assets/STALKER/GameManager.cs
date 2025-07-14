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

    private GameState _currentState; // = GameState.Intro; - здесь не срабатывает
    private GameState _stateBeforeMenu; // Храним предыдущее состояние перед выходом в меню по Esc

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
                Debug.Log("Current State 1 :" + _currentState); // срабатывает только при смене
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
        _currentState = GameState.Intro;
        Debug.Log("Current State : " + _currentState);

        // Запускаем корутину для загрузки меню с задержкой
        StartCoroutine(LoadMainMenuWithDelay());
    }
    #endregion

    private IEnumerator LoadMainMenuWithDelay()
    {
        // Ждем 3 секунды для показа заставки
        yield return new WaitForSeconds(3f);

        // Загружаем MainMenu_P как единственную сцену
        SceneManager.LoadScene(MainMenuSceneName, LoadSceneMode.Single);
        Debug.Log("MainMenu loaded after splash screen");
    }

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
    /// Возвращает в главное меню (без перезагрузки сцены, просто активирует UI)
    /// Возвращает в игру
    /// </summary>
    public void ReturnToMenu()
    {
        // 1. Сохраняем текущее состояние перед переходом в меню
        _stateBeforeMenu = CurrentState;

        // 2. Выгружаем игровые сцены
        UnloadAllGameScenes();

        // 3. Активируем главное меню
        var mainMenuScene = SceneManager.GetSceneByName(MainMenuSceneName);
        if (!mainMenuScene.isLoaded)
        {
            SceneManager.LoadScene(MainMenuSceneName, LoadSceneMode.Single);
        }
        else
        {
            SceneManager.SetActiveScene(mainMenuScene);
        }

        // 4. Устанавливаем состояние меню
        CurrentState = GameState.MainMenu;
        OnReturnedToMenu?.Invoke();

        // 5. Обновляем UI
        UIManager.Instance?.ShowMainMenu();
    }

    /// <summary>
    /// Возвращает в игру
    /// </summary>
    public void ReturnToGame()
    {
        // Возвращаемся в предыдущее состояние
        if (_stateBeforeMenu == GameState.InGameMenuAutoPaused ||
            _stateBeforeMenu == GameState.InGameMenuManualPaused)
        {
            CurrentState = _stateBeforeMenu;
            UIManager.Instance?.ShowPauseMenu(_stateBeforeMenu);
        }
        else
        {
            // Если не было паузы, возвращаемся в обычный геймплей
            CurrentState = GameState.Gameplay;
            Time.timeScale = 1f;
            UIManager.Instance?.HideAllPauseMenus();
        }
    }
    #endregion

    #region Scene Management
    /// <summary>
    /// Загружает главное меню (не аддитивно, выгружая все остальные сцены)
    /// </summary>
    public void LoadMainMenuScene()
    {
        SceneManager.LoadScene(MainMenuSceneName, LoadSceneMode.Single);
        CurrentState = GameState.MainMenu;
        Debug.Log($"MainMenu loaded. CurrentState: {CurrentState}");
    }

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

        // Ждем еще один кадр, чтобы убедиться, что сцена полностью загружена
        yield return null;

        Scene loadedScene = SceneManager.GetSceneByName(sceneName);
        if (!loadedScene.IsValid() || !loadedScene.isLoaded)
        {
            Debug.LogError($"Failed to load scene: {sceneName}");
            yield break;
        }

        SceneManager.SetActiveScene(loadedScene);
        AddGameScene(sceneName);

        CurrentState = GameState.Gameplay;
        Debug.Log($"Scene loaded: {sceneName}, State changed to: {CurrentState}");
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