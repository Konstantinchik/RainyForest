using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimplePlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 200f;

    private CharacterController controller;

    private void OnEnable()
    {
        Time.timeScale = 1f;
        
    }

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
            Cursor.lockState = CursorLockMode.Confined;
            //GameManager.Instance.ChangeState(GameManager.GameState.InGameMenuAutoPaused);
        }

        TogglePause();
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

    public void TogglePause()
    {
        if (Input.GetKeyDown(KeyCode.Pause))
        {
            if(GameManager.Instance.CurrentState == GameManager.GameState.GamePaused)
            {
                Time.timeScale = 1f;
                GameManager.Instance.ChangeState(GameManager.GameState.Gameplay);
                UITestLevel.Instance.HidePauseUI(); // 👈 правильно: СКРЫВАЕМ паузу
                //Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Time.timeScale = 0f;
                GameManager.Instance.ChangeState(GameManager.GameState.GamePaused);
                UITestLevel.Instance.ShowPauseUI(); // 👈 правильно: ПОКАЗЫВАЕМ паузу
                //Cursor.lockState = CursorLockMode.Confined;
            }
        }
    }
}
