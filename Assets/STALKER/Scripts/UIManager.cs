using System;
using UnityEngine;
//using UnityEngine.Rendering;
using static GameManager;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("UI Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject loadGamePanel;
    [SerializeField] private GameObject saveGamePanel;
    [SerializeField] private GameObject lastSavedGamePanel;
    [SerializeField] private GameObject leaveGamePanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private GameObject exitPanel;

    [Header("Misc UI Panels")]
    [SerializeField] private GameObject confirmLostUnsavedGamePanel;

    #region [Awake singleton]
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject); - меню существует только в MainMenu_P
    }
    #endregion

    #region [OnEnable / OnDisable +- HandleGameStateChanged]
    private void OnEnable()
    {
        // Подписываемся через экземпляр GameManager
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        // Отписываемся, если GameManager существует
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }
    #endregion


    private void Start()
    {
        // Получаем текущее состояние при старте
        UpdateUI(GameManager.Instance.CurrentState);
    }

    private void HandleGameStateChanged(GameState previousState, GameState newState)
    {
        UpdateUI(newState);
    }

    private void UpdateUI(GameState state)
    {
        switch (state)
        {
            case GameState.Intro:
                HideAll();
                break;

            case GameState.MainMenu:
                HideAll();
                mainMenuPanel?.SetActive(true);
                break;

            //case GameState.Gameplay:  - обрабатывается в UITestLevel
            //    HideAll();
            //    hudPanel?.SetActive(true);
            //    break;

            //case GameState.GamePaused:   - обрабатывается в UITestLevel
            case GameState.InGameMenuAutoPaused:
            case GameState.InGameMenuManualPaused:
                HideAll();
                mainMenuPanel?.SetActive(true);
                break;
        }
    }

    #region [LoadGameMenu]
    public void ShowLoadGameMenu()
    {
        GameManager.Instance.MainMenu = false;
        loadGamePanel?.SetActive(true);
    }

    public void HideLoadGameMenu()
    {
        loadGamePanel?.SetActive(false);
    }
    #endregion

    #region [SaveGameMenu]
    public void ShowSaveGameMenu()
    {
        GameManager.Instance.MainMenu = false;
        saveGamePanel?.SetActive(true);
    }

    public void HideSaveGameMenu()
    {
        saveGamePanel?.SetActive(false);
    }
    #endregion

    #region [LastSavedGameMenu]
    public void ShowLastSavedGameMenu()
    {
        lastSavedGamePanel?.SetActive(true);
    }

    public void HideLastSavedGameMenu()
    {
        lastSavedGamePanel?.SetActive(false);
    }
    #endregion

    #region [LeaveGameMenu]
    public void ShowLeaveGameMenu()
    {
        GameManager.Instance.MainMenu = false;
        leaveGamePanel?.SetActive(true);
    }

    public void HideLeaveGameMenu()
    {
        leaveGamePanel?.SetActive(false);
    }
    #endregion

    #region [OptionsMenu]
    public void ShowOptionsMenu()
    {
        GameManager.Instance.MainMenu = false;
        optionsPanel?.SetActive(true);
    }

    public void HideOptionsMenu()
    {
        optionsPanel?.SetActive(false);
    }
    #endregion

    #region [CreditsMenu]
    public void ShowCreditsMenu()
    {
        GameManager.Instance.MainMenu = false;
        creditsPanel?.SetActive(true);
    }

    public void HideCreditsMenu()
    {
        GameManager.Instance.MainMenu = true;
        creditsPanel?.SetActive(false);
    }
    #endregion


    
    #region [ExitMenu]
    public void ShowExitMenu()
    {
        exitPanel?.SetActive(true);
    }

    public void HideExitMenu()
    {
        exitPanel?.SetActive(false);
    }
    #endregion

    #region [Confirm Loast Unsaved Data and Start New Game]
    public void ShowConfirmLostUnsavedGameMenu()
    {
        GameManager.Instance.MainMenu = false;
        confirmLostUnsavedGamePanel?.SetActive(true);
    }

    public void HideConfirmLostUnsavedGameMenu()
    {
        GameManager.Instance.MainMenu = true;
        confirmLostUnsavedGamePanel?.SetActive(false);
    }
    #endregion

    private void HideAll()
    {
        mainMenuPanel?.SetActive(false);
        loadGamePanel?.SetActive(false);
        saveGamePanel?.SetActive(false);
        lastSavedGamePanel?.SetActive(false);
        optionsPanel?.SetActive(false);
        creditsPanel?.SetActive(false);
        exitPanel?.SetActive(false);
    }

    public void ShowMainMenu()
    {
        HideAll();
        GameManager.Instance.MainMenu = true;
        mainMenuPanel?.SetActive(true);
    }
}