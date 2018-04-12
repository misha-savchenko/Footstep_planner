using UnityEngine;
using UnityEngine.SceneManagement;

public class restart : MonoBehaviour
{
    
    void Update()
    {
        var global_t_rrt = this.GetComponent<trrt_interpolation>();

        var footsteps = this.GetComponent<footsteps7>();

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
            if (!global_t_rrt.path_found)
            {
                if (global_t_rrt.go)
                {
                    global_t_rrt.go = false;
                }
                else
                {
                    global_t_rrt.go = true;
                }
            }
            else
            {
                if (footsteps.go)
                {
                    footsteps.go = false;
                }
                else
                {
                    footsteps.go = true;
                }
            }


        }
    }
}