using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseClick : MonoBehaviour
{
    public GameManager gm;
    public CameraManager cm;
    public Camera topCam;
    public Collider2D col;
    public AudioManager am;

    public GameObject button;
    // Start is called before the first frame update
    void Start()
    {
        am = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        button.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        // Handle mouse clicks (for PC/Editor)
        if (Input.GetMouseButtonDown(0))
        {
            if (CheckRaycastHit(Input.mousePosition))
            {
                StartCoroutine(IsPressed());
            }
        }

        // Handle touch input (for mobile/tablet)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (CheckRaycastHit(touch.position))
                {
                    StartCoroutine(IsPressed());
                }
            }
        }
    }

    /// <summary>
    /// Checks if a screen position hits the house view button collider
    /// Works with both mouse and touch input
    /// </summary>
    private bool CheckRaycastHit(Vector3 screenPosition)
    {
        Vector3 worldPos = topCam.ScreenToWorldPoint(screenPosition);
        Vector2 worldPos2D = new Vector2(worldPos.x, worldPos.y);

        RaycastHit2D hit = Physics2D.Raycast(worldPos2D, Vector2.zero);

        if (hit.collider == col)
        {
            Debug.Log(hit.collider.gameObject.name);
            return true;
        }
        return false;
    }

    IEnumerator IsPressed()
    {
        col.enabled = false;
        //am.Play("Click");
        cm.HouseView();
        button.SetActive(true);
        yield return new WaitForSeconds(0.25f);
        if (gm.houseList.Count != 0)
        {
            int houseScore = 0;
            string winningTeamName = gm.houseList[0].rockInfo.teamName;
            bool stopCounting = false;

            // lets loop the list
            for (int i = 0; i < gm.houseList.Count; i++)
            {
                if (!stopCounting)
                {
                    // lets only count until the team changes
                    if (gm.houseList[i].rockInfo.teamName == winningTeamName)
                    {
                        houseScore++;
                    }
                    else if (gm.houseList[i].rockInfo.teamName != winningTeamName)
                    {
                        stopCounting = true;
                    }
                }
            }
            bool noRocks = false;

            //send the info to the ui
            gm.gHUD.CheckScore(noRocks, winningTeamName, houseScore, true);
            
        }
        else
            gm.gHUD.CheckScore(true, " ", 0, true);

        gm.gHUD.clickDisplay.enabled = false;
        col.enabled = true;
    }


}
