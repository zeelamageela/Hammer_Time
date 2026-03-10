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

        if (gm.shooterGO)
        {
            if (rb.position.y >= 5f)
            {
                float weightAccu = gm.shooterGO.GetComponent<CharacterStats>().weightAccuracy.GetValue();
                transform.localScale = new Vector3(1.25f - ((0.5f * (weightAccu / 10f)) + 0.25f), 1.25f - ((0.5f * (weightAccu / 10f)) + 0.25f), 0f);
            }
            else
            {

                float finesseAccu = gm.shooterGO.GetComponent<CharacterStats>().finesseAccuracy.GetValue();
                transform.localScale = new Vector3(1.25f - ((0.5f * (finesseAccu / 10f)) + 0.25f), 1.25f - ((0.5f * (finesseAccu / 10f)) + 0.25f), 0f);
            }
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
