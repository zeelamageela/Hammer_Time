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

    private void Start()
    {
        col = GetComponent<BoxCollider2D>();
    }
    public void AttachToRock(GameObject rock)
    {
        rockRB = rock.GetComponent<Rigidbody2D>();
        //rj.connectedBody = rockRB;
    }

    private void Update()
    {
        if (rockRB != null)
        {
            Vector3 followSpot = new Vector3((xOffset + rockRB.position.x), (rockRB.position.y + yOffset), 0f);
            transform.position = followSpot;

            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);

                if (hit.collider == col)
                {
                    Debug.Log(hit.collider.gameObject.name);
                }
            }
        }
    }
    public void Sweep()
    {
        anim.SetBool("Sweep", true);
        anim.SetBool("Hard", false);
    }

    public void Hard()
    {
        anim.SetBool("Sweep", false);
        anim.SetBool("Hard", true);
    }

    public void Whoa()
    {
        anim.SetBool("Sweep", false);
        anim.SetBool("Hard", false);
    }
}
