using UnityEngine;

public class UITestLevel : MonoBehaviour
{
    public static UITestLevel Instance { get; private set; }

    [SerializeField] private GameObject HudPanel;
    [SerializeField] private GameObject PausePanel;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    public void ShowPauseUI()
    {
        PausePanel.SetActive(true);
    }

    public void HidePauseUI()
    {
        PausePanel.SetActive(false);
    }

    void Start()
    {
        
    }

    
    void Update()
    {
        
    }
}
