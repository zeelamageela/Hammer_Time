using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGManager : MonoBehaviour
{
    public GameManager gm;
    public CameraManager cm;
    GameSettingsPersist gsp;

    public GameObject[] boards;
    public GameObject[] ice;
    public Color[] camBG;


    // Start is called before the first frame update
    void Awake()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();

        //Instantiate(boards[gsp.bg], transform);
        Instantiate(ice[gsp.bg], transform);
        cm.main.backgroundColor = camBG[gsp.bg];
        cm.house.backgroundColor = camBG[gsp.bg];
        //gm.boardCollider = boards[gsp.bg].GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
