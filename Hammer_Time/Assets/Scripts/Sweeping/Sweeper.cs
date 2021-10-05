using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sweeper : MonoBehaviour
{
    public Animator anim;
    //public RelativeJoint2D rj;
    Rigidbody2D rockRB;
    public float xOffset;
    public float yOffset;
    Collider2D col;
    public bool sweep;
    public bool hard;
    public bool whoa;

    Transform sweeperParent;
    private void Start()
    {
        col = GetComponent<BoxCollider2D>();

    }

    private void Update()
    {

        //sweeperParent = transform.parent;

        //Vector3 followSpot = new Vector3((xOffset + sweeperParent.position.x), (sweeperParent.position.y + yOffset), 0f);
        //transform.position = followSpot;

        //Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        //if (Input.GetMouseButtonDown(0))
        //{
        //    RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

        //    if (hit.collider == col)
        //    {
        //        if (sweep)
        //        {
        //            Debug.Log("Hard!");
        //            Hard();

        //            if (hard)
        //            {
        //                Debug.Log("Whoa!");
        //                Whoa();
        //            }
        //        }
        //        else
        //        {
        //            Debug.Log("Sweep!");
        //            Sweep();
        //        }
                    
        //        Debug.Log(hit.collider.gameObject.name);
        //    }
        //}
    }
    public void Sweep()
    {
        sweep = true;
        hard = false;
        whoa = false;
        anim.SetBool("Sweep", true);
        anim.SetBool("Hard", false);
    }

    public void Hard()
    {
        sweep = false;
        hard = true;
        whoa = false;
        anim.SetBool("Sweep", false);
        anim.SetBool("Hard", true);
    }

    public void Whoa()
    {
        whoa = true;
        sweep = false;
        hard = false;
        anim.SetBool("Sweep", false);
        anim.SetBool("Hard", false);
    }
}
