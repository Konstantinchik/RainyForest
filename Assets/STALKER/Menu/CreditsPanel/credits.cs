using UnityEngine;

public class credits : MonoBehaviour
{
    void Start()
    {
        
    }

 
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            gameObject.SetActive(false);
        }      
    }
}
