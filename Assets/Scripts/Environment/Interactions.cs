using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Interactions : MonoBehaviour
{

    public GameObject groundPlane;
    public Camera camera;


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            RaycastHit hit;
            Ray ray = camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit))
            {
                Transform objectHit = hit.transform;

                if (objectHit == groundPlane)
                {

                }
            }
        }

    }
}
