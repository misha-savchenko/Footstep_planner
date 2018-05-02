using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public float flySpeed = 0.5f;
    public GameObject defaultCam;
    public GameObject playerObject;
    private bool isEnabled;
    private bool shift;
    private bool ctrl;
    public float accelerationAmount = 3f;
    public float accelerationRatio = 1f;
    public float slowDownRatio = 0.5f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
        {
            shift = true;
            flySpeed *= accelerationRatio;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift))
        {
            shift = false;
            flySpeed /= accelerationRatio;
        }
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            ctrl = true;
            flySpeed *= slowDownRatio;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl) || Input.GetKeyUp(KeyCode.RightControl))
        {
            ctrl = false;
            flySpeed /= slowDownRatio;
        }
        if (Input.GetAxis("Vertical") != 0)
        {
            transform.Translate(-defaultCam.transform.forward * flySpeed * Input.GetAxis("Vertical"));
        }
        if (Input.GetAxis("Horizontal") != 0)
        {
            transform.Translate(-defaultCam.transform.right * flySpeed * Input.GetAxis("Horizontal"));
        }
        if (Input.GetKey(KeyCode.E))
        {
            transform.Translate(defaultCam.transform.up * flySpeed * 0.5f);
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            transform.Translate(-defaultCam.transform.up * flySpeed * 0.5f);
        }
        if (Input.GetKeyDown(KeyCode.F12))
            switchCamera();
        if (Input.GetKeyDown(KeyCode.M))
            playerObject.transform.position = transform.position; //Moves the player to the flycam's position. Make sure not to just move the player's camera.
    }

    void switchCamera()
    {
        if (!isEnabled) //means it is currently disabled. code will enable the flycam. you can NOT use 'enabled' as boolean's name.
        {
            transform.position = defaultCam.transform.position; //moves the flycam to the defaultcam's position
            defaultCam.GetComponent<Camera>().enabled = false;
            this.GetComponent<Camera>().enabled = true;
            isEnabled = true;
        }
        else if (isEnabled) //if it is not disabled, it must be enabled. the function will disable the freefly camera this time.
        {
            this.GetComponent<Camera>().enabled = false;
            defaultCam.GetComponent<Camera>().enabled = true;
            isEnabled = false;
        }
    }
}
