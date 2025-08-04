using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveGameItem : MonoBehaviour
{
    [SerializeField] private Button selectButton;
    [SerializeField] private TMP_Text saveNameText;
    [SerializeField] private TMP_Text timeText;

    private SaveGamePanelController panelController;
    private LoadGamePanelController loadPanelController;
    private string saveName;

    // Перегрузка чтобы можно было использовать из Load и Save
    public void Initialize(string name, string time, SaveGamePanelController controller)
    {
        saveName = name;
        saveNameText.text = name;
        timeText.text = time;
        panelController = controller;

        selectButton.onClick.AddListener(OnItemClicked);
    }

    public void Initialize(string saveName, string time, LoadGamePanelController panel)
    {
        this.saveName = saveName;
        this.loadPanelController = panel;
        saveNameText.text = saveName;

        selectButton.onClick.RemoveAllListeners();
        selectButton.onClick.AddListener(() => loadPanelController.OnSaveItemSelected(saveName));
    }

    private void OnItemClicked()
    {
        panelController.OnSaveItemSelected(saveName);
    }
}