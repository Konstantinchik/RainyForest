using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ControlTab : MonoBehaviour
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


    void Update()
    {
        
    }
}
