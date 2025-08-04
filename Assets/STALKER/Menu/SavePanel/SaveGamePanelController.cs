using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class SaveGamePanelController : MonoBehaviour
{

    [Header("UI References")]
    [SerializeField] private TMP_InputField saveGameInputField;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject confirmOverridePanel;
    [SerializeField] private Transform saveScrollContent;
    [SerializeField] private GameObject saveGameItemPrefab;

    private string selectedSaveName;
    private List<GameObject> saveItems = new List<GameObject>();

    private void Awake()
    {
        saveButton.onClick.AddListener(OnSaveClicked);
        deleteButton.onClick.AddListener(OnDeleteClicked);
        cancelButton.onClick.AddListener(OnCancelClicked);
        saveGameInputField.onSubmit.AddListener(OnSaveInputSubmit);
    }

    private void OnEnable()
    {
        RefreshSaveList();
        saveGameInputField.text = "";
        confirmOverridePanel.SetActive(false);
        selectedSaveName = null;
    }

    private void RefreshSaveList()
    {
        // Очистка старых элементов
        foreach (var item in saveItems)
        {
            Destroy(item);
        }
        saveItems.Clear();

        // Создание новых элементов
        foreach (string saveName in SaveSystemTest.GetSaveFiles())
        {
            GameObject item = Instantiate(saveGameItemPrefab, saveScrollContent);
            SaveGameItem itemScript = item.GetComponent<SaveGameItem>();

            // Тестовые данные (замените на реальные из вашей системы)
            itemScript.Initialize(saveName, "11:10 05/05/2012", this);

            saveItems.Add(item);
        }
    }

    public void OnSaveItemSelected(string saveName)
    {
        selectedSaveName = saveName;
        saveGameInputField.text = saveName;
    }

    private void OnSaveInputSubmit(string text)
    {
        OnSaveClicked();
    }

    private void OnSaveClicked()
    {
        string saveName = saveGameInputField.text;
        if (string.IsNullOrEmpty(saveName) && string.IsNullOrEmpty(selectedSaveName)) return;

        if (SaveSystemTest.SaveExists(saveName) && saveName == selectedSaveName)
        {
            // Показываем панель с callback
            confirmOverridePanel.GetComponent<ConfirmOverridePanel>().Show(ConfirmOverride);
        }
        else
        {
            PerformSave(saveName);
        }
    }

    public void ConfirmOverride(bool confirm)
    {
        if (confirm)
        {
            PerformSave(saveGameInputField.text);
        }
    }

    private void PerformSave(string saveName)
    {
        // Здесь создаем данные для сохранения
        var saveData = new
        {
            playerName = "TestPlayer",
            level = 1,
            timestamp = DateTime.Now.ToString()
        };

        SaveSystemTest.SaveGame(saveName, saveData);
        RefreshSaveList();
        saveGameInputField.text = "";
        selectedSaveName = null;
        Debug.Log($"[Save] Saving game as '{saveName}'");
    }

    private void OnDeleteClicked()
    {
        if (!string.IsNullOrEmpty(selectedSaveName))
        {
            SaveSystemTest.DeleteSave(selectedSaveName);
            RefreshSaveList();
            saveGameInputField.text = "";
            selectedSaveName = null;
        }
    }

    private void OnCancelClicked()
    {
        gameObject.SetActive(false);
    }
}