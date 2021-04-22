using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseCamera : MonoBehaviour
{
    // Start is called before the first frame update
    
    public Camera cam;

    void Start()
    {
        // Set this camera to render after the main camera
        cam.depth = Camera.main.depth - 1;
    }

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            Debug.Log("House Look");
            cam.depth = Camera.main.depth + 1;
        }

        if (Input.GetKeyUp("space"))
        {
            cam.depth = Camera.main.depth - 1;
        }
    }
}
