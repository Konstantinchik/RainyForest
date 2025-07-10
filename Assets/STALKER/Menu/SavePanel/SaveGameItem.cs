using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SaveGameItem : MonoBehaviour
{
    [SerializeField] private Button saveTextButton;
    [SerializeField] private TMP_Text saveNameText;
    [SerializeField] private TMP_Text timeText;

    private SaveGamePanelController panelController;
    private string saveName;

    public void Initialize(string name, string time, SaveGamePanelController controller)
    {
        saveName = name;
        saveNameText.text = name;
        timeText.text = time;
        panelController = controller;

        saveTextButton.onClick.AddListener(OnItemClicked);
    }

    private void OnItemClicked()
    {
        panelController.OnSaveItemSelected(saveName);
    }
}