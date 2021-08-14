using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMan : MonoBehaviour
{
    Animator anim;
    public GameManager gm;

    public string clipName;
    public float distance;
    public GameObject rock;
    public Rock_Info rockInfo;
    public Rigidbody2D rockRB;

    public float max;
    public float min;

    // Start is called before the first frame update
    void Start()
    {

        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gm.rockList.Count != 0)
        {
            rock = gm.rockList[gm.rockCurrent].rock;
            rockInfo = gm.rockList[gm.rockCurrent].rockInfo;
            rockRB = rock.GetComponent<Rigidbody2D>();
            distance = 1f - ((-1f * ((rockRB.position.y - min) / (max - min))) + 1f);
        }
        //distance = Mathf.Clamp(distance, 0f, 1f);

        if (rockInfo.shotTaken)
            anim.SetBool("Active", true);
        else
            anim.SetBool("Active", false);

        anim.Play(clipName, 0, distance);
    }
}
