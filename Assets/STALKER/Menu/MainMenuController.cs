using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuController : MonoBehaviour
{
    [Header("Main Menu References")]
    [SerializeField] private Button newGameButton;      // без панели запроса
    [SerializeField] private Button resumeButton;       // без панели запроса
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button saveGameButton;
    [SerializeField] private Button lastSaveButton;     // Загрузить последнее сохранение?
    [SerializeField] private Button leaveGameButton;    // Желаете покинуть игру?
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button creditsButton;
    [SerializeField] private Button exitButton;

    [Header("Exit Menu References")]
    [SerializeField] private Button confirmExitButton;
    [SerializeField] private Button cancelExitButton;

    [Header("Settings firstLevelName")]
    [SerializeField] private string firstLevelName; // = "Test1"; // берем из GameManager

    private void Awake()
    {
        // Инициализация кнопок
        newGameButton.onClick.AddListener(StartNewGame);

        resumeButton.onClick.AddListener(ResumeGame);

        loadGameButton.onClick.AddListener(ShowLoadGame);

        saveGameButton.onClick.AddListener(ShowSaveGame);

        lastSaveButton.onClick.AddListener(ShowLastSavedGame);

        leaveGameButton.onClick.AddListener(ShowLeaveGame);     // Открывается панель: Покинут игру если PlayMode = GamePlay || GamePause

        optionsButton.onClick.AddListener(ShowOptions);         // Открываем панель настроек

        creditsButton.onClick.AddListener(ShowCredits);

        exitButton.onClick.AddListener(ShowExitConfirm);        // Открывается панель подтверждения выхода
        cancelExitButton.onClick.AddListener(HideExitConfirm);  // Панель подтверждения закрывается
        confirmExitButton.onClick.AddListener(ExitGame);        // Закрытие приложения

        //optionsBackButton.onClick.AddListener(HideOptions);
    }

    private void OnEnable()
    {
        Debug.LogError("MainMenu Enabled");

        // Проверяем доступность кнопки продолжения
        if (GameManager.Instance.CurrentState == GameManager.GameState.MainMenu)
            resumeButton.interactable = false;
        else
            resumeButton.interactable = true;

        // Проверяем доступность кнопки покидания игры
        if (GameManager.Instance.CurrentState == GameManager.GameState.MainMenu)
            leaveGameButton.interactable = false;
        else
            leaveGameButton.interactable = true;
    }

    private void OnDisable()
    {
        Debug.LogError("MainMenu Disabled");
    }

    private void Start()
    {
        firstLevelName = GameManager.Instance.GetFirstGameSceneName;
    }

    #region [Start New Game]
    private void StartNewGame()
    {
        // Если игра уже в процессе, надо спросить согласны ли вы потерять несохраненные данные
        if(GameManager.Instance.CurrentState != GameManager.GameState.MainMenu)
        {
            UIManager.Instance.ShowConfirmLostUnsavedGameMenu();
        }
        else
        // Загружаем сцену через GameManager
        GameManager.Instance.LoadGameScene(firstLevelName);
    }
    #endregion


    #region [Resume Game]
    private void ResumeGame()
    {
        GameManager.Instance.ReturnToGame();
    }
    #endregion

    #region [Load Game UI Panel]
    private void ShowLoadGame()
    {
        UIManager.Instance.ShowLoadGameMenu();
    }
    private void HideLoadGame()
    {
        UIManager.Instance.HideLoadGameMenu();
    }
    #endregion

    #region [Save Game UI Panel]
    private void ShowSaveGame()
    {
        UIManager.Instance.ShowSaveGameMenu();
    }
    private void HideSaveGame()
    {
        UIManager.Instance.HideSaveGameMenu();
    }
    #endregion

    #region [Last Saved Game UI Panel]
    private void ShowLastSavedGame()
    {
        //UIManager.Instance.ShowSaveGameMenu();
    }
    private void HideLastSavedGame()
    {
        UIManager.Instance.ShowSaveGameMenu();
    }
    #endregion

    #region [Leave Game UI Panel]
    private void ShowLeaveGame()
    {
        UIManager.Instance.ShowLeaveGameMenu();
    }
    private void HideLeaveGame()
    {
        UIManager.Instance.HideLeaveGameMenu();
    }
    #endregion

    #region [Options UI Panel]
    private void ShowOptions()
    {
        UIManager.Instance.ShowOptionsMenu();
    }

    private void HideOptions()
    {
        UIManager.Instance.HideOptionsMenu();
    }
    #endregion

    #region [Creditss UI Panel]
    private void ShowCredits()
    {
        UIManager.Instance.ShowCreditsMenu();
    }

    private void HideCredits()
    {
        UIManager.Instance.HideCreditsMenu();
    }
    #endregion

    #region [Exit Confirm UI Panel]
    private void ShowExitConfirm()
    {
        UIManager.Instance.ShowExitMenu();
    }

    private void HideExitConfirm()
    {
        UIManager.Instance.HideExitMenu();
    }
    #endregion

    #region [ExitGame - action]
    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Здесь нужно проверить ... а может и нет
        Application.Quit();
#endif
    }
    #endregion
}