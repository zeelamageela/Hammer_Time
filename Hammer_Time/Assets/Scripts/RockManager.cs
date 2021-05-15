using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockManager : MonoBehaviour
{
    public GameManager gm;
    public CameraManager cm;
    public RockBar rb;
    public GameObject rock;

    public bool inturn = true;

    void OnEnable()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        if (gm.rockList.Count != 0)
        {
            rock = gm.rockList[gm.rockCurrent].rock;
            
            if (inturn)
            {
                rock.GetComponent<Rock_Force>().flipAxis = true;
            }
            else
            {
                rock.GetComponent<Rock_Force>().flipAxis = false;
            }
        }
    }
}
