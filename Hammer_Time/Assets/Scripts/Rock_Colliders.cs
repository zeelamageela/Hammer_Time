using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock_Colliders : MonoBehaviour
{
    public Collider2D InPlay_Collider;
    public Collider2D boards_collider;
    public Collider2D house_collider;
    public Transform button_measure;

    private Rigidbody2D body;

    public bool outOfPlay = false;
    public bool inPlay = false;


// Start is called before the first frame update
    void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        GameObject InPlay = GameObject.Find("InPlay_Collider");
        InPlay_Collider = InPlay.GetComponent<Collider2D>();

        GameObject boards = GameObject.Find("Grid/Boards");
        boards_collider = boards.GetComponent<Collider2D>();

    }

// Update is called once per frame
    void Update()
    {
        if (outOfPlay != false && GetComponent<Rock_Release>().released)
        {
            StartCoroutine(OutOfPlay());
        }
    }


    IEnumerator OutOfPlay()
    {
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;

        GetComponent<Rock_Info>().outOfPlay = true;
        GetComponent<Rock_Info>().stopped = true;
        GetComponent<Rock_Info>().inHouse = false;
        yield break;
    }


    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == InPlay_Collider)
        {
            Debug.Log("rock is in play");
            inPlay = true;
            GetComponent<Rock_Info>().inPlay = true;
        }

        if (collider == boards_collider && GetComponent<Rock_Release>().released == true)
        {
            outOfPlay = true;
            GetComponent<Rock_Info>().outOfPlay = outOfPlay;
            inPlay = false;
            GetComponent<Rock_Info>().inPlay = inPlay;
            Debug.Log("trigger boards");
            GetComponent<Rock_Info>().inHouse = false;
            GetComponent<Rock_Info>().stopped = true;
        }

        if (collider.gameObject.tag == "House")
        {
            GetComponent<Rock_Info>().inHouse = true;
            
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Rock")
        {
            GetComponent<Rock_Info>().hit = true;
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {

        //Check for a match with the specified name on any GameObject that collides with your GameObject
        if (collider == InPlay_Collider)
        {
            Debug.Log("Out");
            outOfPlay = true;
            GetComponent<Rock_Info>().outOfPlay = true;
            inPlay = false;
            GetComponent<Rock_Info>().inPlay = false;
            GetComponent<Rock_Info>().stopped = true;
        }

        if (collider.gameObject.tag == "House")
        {
            GetComponent<Rock_Info>().inHouse = false;
        }
    }

}
