using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreboardCheck : MonoBehaviour
{
    public GameManager gm;
    public GameHUD gHUD;
    public Camera uiCam;
    public Collider2D col;
    public PauseMenu pm;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = uiCam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider == col)
            {
                gHUD.scoreCheck = true;
                Debug.Log(hit.collider.gameObject.name);
                pm.Pause();
            }
        }
    }
}
