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

    public GameObject MainMenuRoot { get; private set; }

    public bool MainMenu;       // Если мы переходим в другие меню = false. При нажатии на Esc = true, ShowMainMenu()

    private GameState _currentState;    // = GameState.Intro; - здесь не срабатывает
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
    private const string FirstGameSceneName = "Test1";
    public List<string> LoadedGameScenes { get; private set; } = new List<string>();
    private GameObject _currentPlayer;
    #endregion

    public string GetFirstGameSceneName { get {  return FirstGameSceneName; } }

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

    #region [Load Main Menu after Intro]
    private IEnumerator LoadMainMenuWithDelay()
    {
        // Ждем 3 секунды для показа заставки
        yield return new WaitForSeconds(3f);

        // Загружаем MainMenu_P как единственную сцену
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(MainMenuSceneName, LoadSceneMode.Single);

        // Ждем завершения загрузки
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Устанавливаем активную сцену
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainMenu_P"));

        // Обновляем состояние
        ChangeState(GameState.MainMenu);


        Debug.Log($"State changed to: {CurrentState}");
        Debug.Log("MainMenu loaded after splash screen");
    }
    #endregion


    #region [Change Game State]
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        GameState previousState = CurrentState;
        CurrentState = newState;



        if (newState == GameState.Gameplay)
        {
            TryFindPlayer(); // 👈 авто-поиск при переходе в GamePlay
        }

        // 👇 ВАЖНО: при автопаузе сразу показать главное меню
        if ((newState == GameState.InGameMenuAutoPaused) ||(newState == GameState.InGameMenuManualPaused))
        {
            UIManager.Instance?.ShowMainMenu();
        }

        // Уведомляем подписчиков
        OnGameStateChanged?.Invoke(previousState, newState);
        Debug.Log($"State changed from {previousState} to {newState}");
    }
    #endregion

    private void TryFindPlayer()
    {
        if (_currentPlayer == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                _currentPlayer = playerObj;
                NotifyPlayerSpawned(_currentPlayer); // вызывает OnPlayerSpawned
                Debug.Log($"[GameManager] Player found during GamePlay: {_currentPlayer.name}");
            }
            else
            {
                Debug.LogWarning("[GameManager] Player not found in scene.");
            }
        }
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
            // ✅ Не трогаем CurrentState напрямую, используем ChangeState
            if (CurrentState == GameState.GamePaused || CurrentState == GameState.InGameMenuManualPaused)
            {
                ChangeState(GameState.InGameMenuManualPaused);
            }
            else
            {
                ChangeState(GameState.InGameMenuAutoPaused);
            }

            Time.timeScale = 0f;
            Debug.Log($"State changed to: {CurrentState}");
            OnGamePaused?.Invoke();
        }
        else
        {
            CurrentState = GameState.Gameplay;
            Time.timeScale = 1f;
            Debug.Log($"State changed to: {CurrentState}");
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
        HideMainMenuUI();
        Cursor.lockState = CursorLockMode.Locked;

        // Возвращаемся в предыдущее состояние
        if (_stateBeforeMenu == GameState.InGameMenuAutoPaused)
        {
            Time.timeScale = 1f;
            ChangeState(GameState.Gameplay);
            Debug.LogError("Вернулись в игру Gameplay");
        }
        if(_stateBeforeMenu == GameState.InGameMenuManualPaused)
        {
            Time.timeScale = 0f;
            ChangeState(GameState.GamePaused);
            UITestLevel.Instance?.ShowPauseUI(); // 👈 это важно!
            Debug.LogError("Вернулись в игру GamePaused");
        }
    }

    /// <summary>
    /// Покидает текущую игру: выгружает игровые сцены, возвращает в главное меню,
    /// отключает кнопки "Продолжить" и "Покинуть игру"
    /// </summary>
    public void LeaveCurrentGame()
    {
        Debug.Log("[GameManager] Leaving current game...");

        HideMainMenuUI();

        // 1. Выгружаем все игровые сцены
        UnloadAllGameScenes();

        // 2. Деактивируем игрока
        DeactivatePlayer();
        _currentPlayer = null;

        // 3. Возвращаемся в главное меню
        CurrentState = GameState.MainMenu;

        // 4. Отображаем главный UI
        ShowMainMenuUI();
        UIManager.Instance?.ShowMainMenu();

        /*
        // 5. Отключаем кнопки "Продолжить игру" и "Покинуть игру", если такие есть
        var leavePanel = GameObject.Find("LeaveGamePanel");
        if (leavePanel != null)
        {
            var continueBtn = leavePanel.transform.Find("ContinueButton")?.GetComponent<Button>();
            var leaveBtn = leavePanel.transform.Find("LeaveButton")?.GetComponent<Button>();

            if (continueBtn != null) continueBtn.interactable = false;
            if (leaveBtn != null) leaveBtn.interactable = false;
        }
        */

        Debug.Log("[GameManager] Game exited to Main Menu.");
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

        if (scene.name == MainMenuSceneName)
        {
            // Найдём и сохраним ссылку на корень главного меню
            MainMenuRoot = GameObject.Find("MainMenuRoot");
            Debug.LogWarning("[GameManager] MainMenuRoot ДОБАВЛЕН!");
            if (MainMenuRoot == null)
                Debug.LogWarning("[GameManager] MainMenuRoot не найден в сцене MainMenu_P!");
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
        Debug.Log($"[GameManager] Player spawned: {_currentPlayer.name}");

        // Инициализация компонентов игрока
        var stats = _currentPlayer.GetComponent<PlayerStats>();
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
                case GameState.Gameplay:                // Ставим игру на автопаузу и выходим в меню. GameState == InGameMenuAutoPaused
                    ChangeState(GameState.InGameMenuAutoPaused);
                    SetPaused(true);
                    break;
                case GameState.GamePaused:
                    ChangeState(GameState.InGameMenuManualPaused);
                    SetPaused(true);
                    break;
                case GameState.InGameMenuAutoPaused:    // Возвращаемся в игру. Снимаем таймер с паузы. GameState == GamePlay
                    if (MainMenu)
                    {
                        SetPaused(false);
                    }
                    else
                    {
                        UIManager.Instance.ShowMainMenu();
                    }
                    break;
                case GameState.InGameMenuManualPaused:  // Возвращаемся в игру. Панель паузы активена. Таймер на паузе. GameState == GamePaused
                    if (MainMenu)
                    {
                        SetPaused(true);
                    }
                    else
                    {
                        UIManager.Instance.ShowMainMenu();
                    }
                    break;
                case GameState.MainMenu:
                    // Если мы загрузили игру и просто в главном меню, Esc ничего не делает
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

        // ✅ Устанавливаем состояние в Gameplay
        ChangeState(GameState.Gameplay);

        // ✅ Деактивируем MainMenu
        HideMainMenuUI();

        Debug.Log($"Scene loaded: {sceneName}, State changed to: {CurrentState}");
    }
    #endregion

    #region [Activate / Deactivete Player]
    private void DeactivatePlayer()
    {
        if (_currentPlayer != null)
        {
            _currentPlayer.SetActive(false);
            Debug.Log("[GameManager] Player deactivated.");
        }
    }

    private void ActivatePlayer()
    {
        if (_currentPlayer != null)
        {
            _currentPlayer.SetActive(true);
            Debug.Log("[GameManager] Player re-activated.");
        }
    }

    #endregion

    #region [Main Menu Visibility]

    /// <summary>
    /// Активирует корневой объект главного меню (MainMenuRoot)
    /// </summary>
    public void ShowMainMenuUI()
    {
        if (MainMenuRoot != null)
            MainMenuRoot.SetActive(true);
        else
            Debug.LogWarning("[GameManager] MainMenuRoot ещё не присвоен.");

        DeactivatePlayer();
    }

    /// <summary>
    /// Деактивирует корневой объект главного меню (MainMenuRoot)
    /// </summary>
    public void HideMainMenuUI()
    {
        if (MainMenuRoot != null)
            MainMenuRoot.SetActive(false);
        else
            Debug.LogWarning("[GameManager] MainMenuRoot ещё не присвоен.");

        ActivatePlayer();
    }
    #endregion

    public void SaveCurrentGame(string saveName)
    {
        if (_currentPlayer == null)
        {
            Debug.LogError("Player reference is missing!");
            return;
        }

        var data = new GameSaveData
        {
            levelName = SceneManager.GetActiveScene().name,
            playerPosition = _currentPlayer.transform.position,
            timestamp = DateTime.Now
        };
        SaveSystemTest.SaveGame(saveName, data);
        Debug.Log($"Game saved: {saveName} at {data.timestamp}");
    }

    public void LoadGame(string saveName)
    {
        GameSaveData data = SaveSystemTest.LoadGame(saveName);
        if (data == null)
        {
            Debug.LogError($"Save file {saveName} not found!");
            return;
        }
        StartCoroutine(LoadGameCoroutine(data));
    }

    private IEnumerator LoadGameCoroutine(GameSaveData data)
    {
        // Создаем операцию загрузки
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(data.levelName);
        loadOperation.allowSceneActivation = false;

        // Ждем загрузки на 90% (оставшиеся 10% - активация)
        while (loadOperation.progress < 0.9f)
        {
            yield return null;
        }

        // Разрешаем активацию сцены
        loadOperation.allowSceneActivation = true;

        // Ждем завершения
        while (!loadOperation.isDone)
        {
            yield return null;
        }

        // Теперь ищем игрока
        _currentPlayer = GameObject.FindGameObjectWithTag("Player");
        if (_currentPlayer == null)
        {
            Debug.LogError("Player object not found after scene load!");
            yield break;
        }

        // Восстанавливаем позицию
        _currentPlayer.transform.position = data.playerPosition;
        Debug.Log($"Game loaded: {data.levelName}, player at {data.playerPosition}");
    }

}

#region [GameSaveData public serializable class -- OLD]
/// <summary>
/// Данные для сохранения. В будущем будут в отдельном файле
/// </summary>

/*
// using System;
// using UnityEngine;
[Serializable]
public class GameSaveData
{
    public string levelName;      // Имя текущей сцены
    public Vector3 playerPosition; // Позиция игрока
    public DateTime timestamp;    // Время сохранения

    /*
    // Системная информация
    public string saveVersion = "1.0";
    public DateTime saveTime;
    public string saveName;

    // Сцена и прогресс
    public string currentLevel;
    public Vector3 playerPosition;
    public Quaternion playerRotation;

    // Параметры игрока
    public float health;
    public float maxHealth;
    public int experience;
    public int level;

    // Инвентарь (пример)
    public string[] inventoryItems;
    public int[] inventoryCounts;

    // Настройки игры (если нужно)
    public float musicVolume;
    public float effectsVolume;

    // Конструктор для быстрого создания
    public GameSaveData(string name, string level, Vector3 position)
    {
        saveName = name;
        currentLevel = level;
        playerPosition = position;
        saveTime = DateTime.Now;
    }

    // Метод для валидации данных
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(currentLevel)
            && health > 0
            && !string.IsNullOrEmpty(saveVersion);
    }
    
}*/
#endregion