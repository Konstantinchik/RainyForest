using UnityEngine;
using UnityEngine.UI;

public class LeaveGamePanel : MonoBehaviour
{
    [SerializeField] Button confirmLeaveGameButton;
    [SerializeField] Button cancelLeaveGameButton;

    void Start()
    {
        
    }

 
    void Update()
    {
        
    }

    public void ConfirmLeaveGame()
    {
        gameObject.SetActive(false);
        GameManager.Instance.LeaveCurrentGame();
    }

    public void CancelLeaveGame()
    {
        GameManager.Instance.MainMenu = true;
        gameObject.SetActive(false);
    }
}
