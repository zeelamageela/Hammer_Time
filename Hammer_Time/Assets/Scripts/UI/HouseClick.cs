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
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = topCam.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

            if (hit.collider == col)
            {

                StartCoroutine(IsPressed());
                Debug.Log(hit.collider.gameObject.name);
            }
        }
    }

    IEnumerator IsPressed()
    {
        col.enabled = false;

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
            gm.gHUD.CheckScore(noRocks, winningTeamName, houseScore);
            
        }
        else
            gm.gHUD.CheckScore(true, null, 0);

        gm.gHUD.clickDisplay.enabled = false;
        col.enabled = true;
    }


}
