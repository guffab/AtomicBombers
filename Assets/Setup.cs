using UnityEngine;
using UnityEngine.SceneManagement;

public class Setup : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SceneManager.LoadScene("World");
    }
}
