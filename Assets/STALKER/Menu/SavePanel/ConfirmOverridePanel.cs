using UnityEngine;

public class ConfirmOverridePanel : MonoBehaviour
{
    private System.Action<bool> callback;

    // Инициализация панели с callback
    public void Show(System.Action<bool> resultCallback)
    {
        this.callback = resultCallback;
        gameObject.SetActive(true);
    }

    // Вызывается при нажатии "Save" (Подтвердить)
    public void OnConfirmSave()
    {
        callback?.Invoke(true);
        gameObject.SetActive(false);
    }

    // Вызывается при нажатии "Cancel" (Отменить)
    public void OnConfirmCancel()
    {
        callback?.Invoke(false);
        gameObject.SetActive(false);
    }
}
