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

    [Header("Load Game References")]
    [SerializeField] private GameObject loadGamePanel;
    [SerializeField] private Button loadGameBackButton;

    [Header("Save Game References")]
    [SerializeField] private GameObject saveGamePanel;
    [SerializeField] private Button saveGameBackButton;

    [Header("Last Save Game References")]
    [SerializeField] private GameObject lastSaveGamePanel;
    [SerializeField] private Button lastSaveGameBackButton;

    [Header("Options References")]
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private Button optionsBackButton;

    [Header("Exit Confirmation")]
    [SerializeField] private GameObject exitConfirmPanel;
    [SerializeField] private Button exitConfirmButton;
    [SerializeField] private Button exitCancelButton;

    [Header("Settings")]
    [SerializeField] private string firstLevelName = "Level1"; //

    private void Awake()
    {
        // Инициализация кнопок
        newGameButton.onClick.AddListener(StartNewGame);

        resumeButton.onClick.AddListener(ResumeGame);

        loadGameButton.onClick.AddListener(ShowLoadGame);

        saveGameButton.onClick.AddListener(ShowSaveGame);

        lastSaveButton.onClick.AddListener(ShowLastSavedGame);

        leaveGameButton.onClick.AddListener(ShowLeaveGame);

        optionsButton.onClick.AddListener(ShowOptions);
        optionsBackButton.onClick.AddListener(HideOptions);

        creditsButton.onClick.AddListener(ShowCredits);

        exitButton.onClick.AddListener(ShowExitConfirm);
        exitCancelButton.onClick.AddListener(HideExitConfirm);
        exitConfirmButton.onClick.AddListener(ExitGame);


        // Проверяем доступность кнопки продолжения
        resumeButton.interactable = SaveSystem.HasSave();
    }

    #region [Start New Game]
    private void StartNewGame()
    {
        SaveSystem.CreateNewSave();
        // Загружаем сцену через GameManager
        GameManager.Instance.LoadGameScene(firstLevelName);
    }
    #endregion


    #region [Resume Game]
    private void ResumeGame()
    {
        if (SaveSystem.HasSave())
        {
            string lastLevel = SaveSystem.GetLastLevel();
            // Загружаем последний уровень или первый, если сохранение не содержит уровня
            GameManager.Instance.LoadGameScene(!string.IsNullOrEmpty(lastLevel) ? lastLevel : firstLevelName);
        }
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
        UIManager.Instance.ShowSaveGameMenu();
    }
    private void HideLastSavedGame()
    {
        UIManager.Instance.ShowSaveGameMenu();
    }
    #endregion

    #region [Leave Game UI Panel]
    private void ShowLeaveGame()
    {
        UIManager.Instance.ShowSaveGameMenu();
    }
    private void HideLeaveGame()
    {
        UIManager.Instance.ShowSaveGameMenu();
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
        exitConfirmPanel.SetActive(true);
    }

    private void HideExitConfirm()
    {
        exitConfirmPanel.SetActive(false);
    }
    #endregion

    #region [ExitGame - action]
    private void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
    #endregion
}