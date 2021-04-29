using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Debug_Shooting : MonoBehaviour
{
    public GameManager gm;
    GameObject rock;
    Rigidbody2D rb;
    Rock_Flick rockFlick;
    Rock_Info rockInfo;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            rock = gm.rockList[gm.rockCurrent].rock;
            rb = rock.GetComponent<Rigidbody2D>();
            rockFlick = rock.GetComponent<Rock_Flick>();
            rockInfo = rock.GetComponent<Rock_Info>();

            StartCoroutine(HouseShot());
        }

    }

    public void OnHouse()
    {
        rock = gm.rockList[gm.rockCurrent].rock;
        rb = rock.GetComponent<Rigidbody2D>();
        rockFlick = rock.GetComponent<Rock_Flick>();
        rockInfo = rock.GetComponent<Rock_Info>();

        StartCoroutine(HouseShot());
    }


    IEnumerator HouseShot()
    {
        rock.GetComponent<SpringJoint2D>().enabled = false;
        rb.isKinematic = true;

        rock.transform.position = new Vector2(-29f, -0.3f);
        rockFlick.isPressed = true;
        Debug.Log("gonna shoot");

        yield return new WaitForSeconds(0.5f);

        rockFlick.isPressed = false;
        rb.isKinematic = false;
        rock.GetComponent<SpringJoint2D>().enabled = true;
        StartCoroutine(rockFlick.Release());

    }
}
