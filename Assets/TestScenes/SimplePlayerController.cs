using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 200f;

    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        // Поворот только по оси Y (влево/вправо)
        float rotate = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        transform.Rotate(0f, rotate, 0f);

        // Движение вперёд/назад/влево/вправо в локальных координатах
        float h = Input.GetAxis("Horizontal"); // A/D
        float v = Input.GetAxis("Vertical");   // W/S

        Vector3 move = transform.right * h + transform.forward * v;
        controller.SimpleMove(move * moveSpeed);

        if(Input.GetKey(KeyCode.Escape))
        {
            GameManager.Instance.ShowMainMenuUI();
            GameManager.Instance.ChangeState(GameManager.GameState.InGameMenuAutoPaused);
        }
    }

    public void SetControlEnabled(bool enabled)
    {
        this.enabled = enabled; // отключает Update()
        if (TryGetComponent<Camera>(out var cam))
            cam.enabled = enabled;

        // Если камера — дочерний объект
        var childCamera = GetComponentInChildren<Camera>();
        if (childCamera != null)
            childCamera.enabled = enabled;
    }
}
