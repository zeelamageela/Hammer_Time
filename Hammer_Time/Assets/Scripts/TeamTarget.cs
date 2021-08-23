using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamTarget : MonoBehaviour
{ 
    public GameManager gm;
    public CameraManager cm;
    Rigidbody2D rb;
    public Vector2 mousePos;
    bool isPressed;

    // Start is called before the first frame update
    void OnEnable()
    {
        rb = GetComponent<Rigidbody2D>();
        
    }

    private void Update()
    {
        if (isPressed)
        {
            mousePos = cm.house.ScreenToWorldPoint(Input.mousePosition);

            rb.position = mousePos;
        }
        
    }
    // Update is called once per frame
    void OnMouseDown()
    {
        isPressed = true;
    }
    private void OnMouseUp()
    {
        isPressed = false;
    }
}
