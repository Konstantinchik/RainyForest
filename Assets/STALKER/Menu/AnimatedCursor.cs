using UnityEngine;
using UnityEngine.UI;

// Чтобы этот анимированный курсор работал корректно с dropdown нужно
// sorting order у Canvas установить 32767 - или выше

public class AnimatedCursor : MonoBehaviour
{
    [SerializeField]
    private Sprite[] cursorFrames;
    public float frameRate = 10f;

    private Image cursorImage;
    private int currentFrame;
    private float timer;

    // НАДО ВОПРОС КОНКРЕТНО ПОРЕШАТЬ
    private Vector2 offset = new Vector2(28, -30); // Смещение вправо и вниз

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Confined; // Или Locked, если нужен захват мыши
    }

    void Start()
    {
        Cursor.visible = false;
        cursorImage = GetComponent<Image>();
        if (cursorFrames.Length > 0)
            cursorImage.sprite = cursorFrames[0];
    }

    void Update()
    {

        transform.position = Input.mousePosition + (Vector3)offset;

        if (cursorFrames == null || cursorFrames.Length == 0)
            return;

        // Используем unscaledDeltaTime, чтобы работало при Time.timeScale = 0
        timer += Time.unscaledDeltaTime;

        if (timer >= 1f / frameRate)
        {

            currentFrame = (currentFrame + 1) % cursorFrames.Length;
            if (currentFrame >= 7) currentFrame = 0;
            cursorImage.sprite = cursorFrames[currentFrame];
            timer = 0f;
        }
    }
}
