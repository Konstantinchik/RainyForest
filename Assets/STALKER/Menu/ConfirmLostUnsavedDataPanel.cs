using UnityEngine;
using System;

public class ConfirmLostUnsavedDataPanel : MonoBehaviour
{
    private Action<bool> callback;

    public void Show(Action<bool> resultCallback)
    {
        callback = resultCallback;
        gameObject.SetActive(true);
    }

    public void OnConfirm()
    {
        callback?.Invoke(true);
        gameObject.SetActive(false);

        GameManager.Instance.LoadGameScene(GameManager.Instance.GetFirstGameSceneName);
    }

    public void OnCancel()
    {
        callback?.Invoke(false);
        gameObject.SetActive(false);
    }
}