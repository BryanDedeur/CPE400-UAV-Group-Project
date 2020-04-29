using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InteractionManager : MonoBehaviour
{

    public float cameraSpeed = 10f;
    private int multiplyer = 1;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            Camera.main.transform.position += new Vector3(1, 0, 0) * cameraSpeed * multiplyer * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.A))
        {
            Camera.main.transform.position += new Vector3(0, 0, 1) * cameraSpeed * multiplyer * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.S))
        {
            Camera.main.transform.position += new Vector3(-1, 0, 0) * cameraSpeed * multiplyer * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.D))
        {
            Camera.main.transform.position += new Vector3(0, 0, -1) * cameraSpeed * multiplyer * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.E))
        {
            Camera.main.orthographicSize += cameraSpeed * multiplyer * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            Camera.main.orthographicSize -= cameraSpeed * multiplyer * Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            multiplyer = 2;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            multiplyer = 1;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (Camera.main.orthographic)
            {
                Camera.main.orthographic = false;
                Camera.main.transform.position = new Vector3(-90, 35, 15);
                Camera.main.transform.eulerAngles = new Vector3(35, 90, 0);
            } else
            {
                Camera.main.orthographic = true;
                Camera.main.transform.position = new Vector3(0, 50, 25);
                Camera.main.transform.eulerAngles = new Vector3(90, 90, 0);
            }
        }
    }


    public void Pause()
    {
        if (Time.timeScale == 0)
        {
            Time.timeScale = 1;
        }
        else
        {
            Time.timeScale = 0;
        }
    }

    public void Reload()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


}
