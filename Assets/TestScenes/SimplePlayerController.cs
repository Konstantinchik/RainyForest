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
    }
}
