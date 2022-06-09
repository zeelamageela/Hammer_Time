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

        gsp.bg = carM.currentTourny.BG;
        randBoards = Instantiate(boards[gsp.bg], transform);
        randBoards.name = "Boards_CREATED";
        Instantiate(ice[gsp.bg], transform);
        cm.main.backgroundColor = camBG[gsp.bg];
        cm.house.backgroundColor = camBG[gsp.bg];
        boards[gsp.bg].GetComponent<Collider2D>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
