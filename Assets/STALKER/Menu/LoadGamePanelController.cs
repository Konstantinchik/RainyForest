using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using static GameManager;

public class LoadGamePanelController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Transform loadScrollContent;
    [SerializeField] private GameObject saveGameItemPrefab;

    [Header("Level Info UI")]
    [SerializeField] private TMP_Text saveNameText;
    [SerializeField] private TMP_Text levelNameText;
    [SerializeField] private TMP_Text dateTimeText;
    [SerializeField] private TMP_Text healthPercentText;

    [Header("")]
    [SerializeField] private Button selectButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button cancelButton;

    [SerializeField] private GameObject confirmLostPanel;

    private string selectedSaveName;
    private List<GameObject> saveItems = new List<GameObject>();

    private void Awake()
    {

        selectButton.onClick.AddListener(OnSelectClicked);
        deleteButton.onClick.AddListener(OnDeleteClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
    }

    private void OnEnable()
    {
        selectedSaveName = null;
        RefreshSaveList();
        ClearInfo();
    }

    private void RefreshSaveList()
    {
        foreach (var item in saveItems)
            Destroy(item);
        saveItems.Clear();

        foreach (string saveName in SaveSystemTest.GetSaveFiles())
        {
            GameObject item = Instantiate(saveGameItemPrefab, loadScrollContent);
            SaveGameItem itemScript = item.GetComponent<SaveGameItem>();

            itemScript.Initialize(saveName, "11:10 05/05/2012", this); // можно загрузить метаданные
            saveItems.Add(item);
        }
    }

    public void OnSaveItemSelected(string saveName)
    {
        selectedSaveName = saveName;
        
        // Получить информацию о сохранении
        var info = SaveSystemTest.LoadMetaData(saveName); // Заменить на реальную загрузку метаинфо

        saveNameText.text = saveName;
        levelNameText.text = info?.levelName ?? "Unknown";
        dateTimeText.text = info?.dateTime ?? "N/A";
        healthPercentText.text = info != null ? $"{info.healthPercent}%" : "0%";
        
    }

    private void OnSelectClicked()
    {
        if (string.IsNullOrEmpty(selectedSaveName)) return;

        var state = GameManager.Instance.CurrentState;
        if (state == GameState.InGameMenuAutoPaused || state == GameState.InGameMenuManualPaused)
        {
            confirmLostPanel.GetComponent<ConfirmLostUnsavedDataPanel>().Show(confirmed =>
            {
                if (confirmed)
                    PerformLoad(selectedSaveName);
            });
        }
        else
        {
            PerformLoad(selectedSaveName);
        }
    }

    private void PerformLoad(string saveName)
    {
        //SaveSystemTest.LoadGame(saveName);     //   !!!!
        Debug.Log(" ИГРА ЗАГРУЖЕНА ");
        gameObject.SetActive(false);
    }

    private void OnDeleteClicked()
    {
        if (string.IsNullOrEmpty(selectedSaveName)) return;

        SaveSystemTest.DeleteSave(selectedSaveName);
        RefreshSaveList();
        selectedSaveName = null;
        ClearInfo();
    }

    private void OnCancelClicked()
    {
        gameObject.SetActive(false);
    }

    private void ClearInfo()
    {
        saveNameText.text = "";
        levelNameText.text = "";
        dateTimeText.text = "";
        healthPercentText.text = "";
    }
}
