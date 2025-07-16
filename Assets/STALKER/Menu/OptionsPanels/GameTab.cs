using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GameTab : MonoBehaviour
{
    [SerializeField] OptionsPanel OptionsPanel;
    [SerializeField] Button videoButton;
    [SerializeField] Button soundButton;
    [SerializeField] Button gameButton;
    [SerializeField] Button controlButton;

    #region [SELECT TAB]
    public void ShowVideoTab()
    {
        OptionsPanel.ShowVideoTab();
    }

    public void ShowSoundTab()
    {
        OptionsPanel.ShowSoundTab();
    }

    public void ShowGameTab()
    {
        OptionsPanel.ShowGameTab();
    }

    public void ShowControlTab()
    {
        OptionsPanel.ShowControlTab();
    }
    #endregion

    #region [Set Save Path]
    private string defaultSavePath
    {
        get
        {
            // поднимаемся на один уровень вверх 
            string folderPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Default");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            return Path.Combine(folderPath, "gamesettings_default.json");
        }
    }

    private string savePath
    {
        get
        {
            // поднимаемся на один уровень вверх 
            string folderPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, "Options");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            return Path.Combine(folderPath, "gamesettings.json");
        }
    }
    #endregion

    void Update()
    {
        
    }
}
