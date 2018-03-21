using UnityEngine;
using UnityEngine.SceneManagement;

public class restart : MonoBehaviour
{

    void Start()
    {
    }

    void Update()
    {
        var go = this.GetComponent<steps_A_E>();


        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        
            {
            if (go.go)
            {
                go.go = false;
            }
            else
            {
                go.go = true;
            }
        }
    }
}