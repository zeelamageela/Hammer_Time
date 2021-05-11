using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootingKnob : MonoBehaviour
{
    public GameManager gm;
    GameObject activeRock;

    public GameObject launcher;

    // Start is called before the first frame update
    public void ParentToRock(GameObject rock)
    {
        GetComponent<SpriteRenderer>().enabled = true;
        transform.parent = rock.transform;
    }

    // Update is called once per frame
    public void UnParentandHide()
    {
        transform.parent = null;
        GetComponent<SpriteRenderer>().enabled = false;
        transform.position = new Vector3(0f, -25f, 0f);
    }
}
