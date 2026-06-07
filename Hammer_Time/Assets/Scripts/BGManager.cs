using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGManager : MonoBehaviour
{
    public GameManager gm;
    public CareerManager carM;
    public CameraManager cm;
    GameSettingsPersist gsp;

    public GameObject[] boards;
    public GameObject[] ice;
    public Color[] camBG;

    public GameObject randBoards;


    // Start is called before the first frame update
    void Awake()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
        carM = FindObjectOfType<CareerManager>();

        int bgIndex = 0;
        if (carM != null && carM.currentTourny != null)
        {
            bgIndex = carM.currentTourny.BG;
        }
        else if (gsp != null)
        {
            bgIndex = gsp.bg;
        }

        if (gsp != null)
        {
            gsp.bg = bgIndex;
        }

        if (bgIndex >= 0 && bgIndex < boards.Length)
        {
            randBoards = Instantiate(boards[bgIndex], transform);
            randBoards.name = "Boards_CREATED";
            boards[bgIndex].GetComponent<Collider2D>().enabled = false;
        }
        else
        {
            Debug.LogWarning($"BGManager: Invalid bgIndex {bgIndex} for boards array.");
        }

        if (bgIndex >= 0 && bgIndex < ice.Length)
        {
            Instantiate(ice[bgIndex], transform);
        }
        else
        {
            Debug.LogWarning($"BGManager: Invalid bgIndex {bgIndex} for ice array.");
        }

        if (cm != null)
        {
            if (cm.main != null)
                cm.main.backgroundColor = camBG[Mathf.Clamp(bgIndex, 0, camBG.Length - 1)];
            if (cm.house != null)
                cm.house.backgroundColor = camBG[Mathf.Clamp(bgIndex, 0, camBG.Length - 1)];
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
