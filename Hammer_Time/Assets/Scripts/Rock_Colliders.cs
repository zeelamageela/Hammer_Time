using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock_Colliders : MonoBehaviour
{
    public Collider2D InPlay_Collider;
    public Collider2D boards_collider;
    public Collider2D house_collider;

    private Rigidbody2D body;

    public bool outOfPlay = false;
    public bool inPlay = false;
    public bool hit = false;
    public bool inHouse = false;


// Start is called before the first frame update
    void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        GameObject InPlay = GameObject.Find("InPlay_Collider");
        InPlay_Collider = InPlay.GetComponent<Collider2D>();

        GameObject boards = GameObject.Find("Grid/Boards");
        boards_collider = boards.GetComponent<Collider2D>();

        GameObject house = GameObject.Find("House");
        house_collider = house.GetComponent<Collider2D>();

    }

// Update is called once per frame
    void Update()
    {
        if (outOfPlay && GetComponent<Rock_Release>().released)
        {
            if (inPlay == false)
            {
                StartCoroutine(OutOfPlay());
            }
        }
    }


    public IEnumerator OutOfPlay()
    {
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;

        yield return new WaitForSeconds(0.4f);
        GetComponent<Rock_Info>().stopped = true;
        GetComponent<Rock_Info>().rest = true;

        gameObject.SetActive(false);

        yield return new WaitForSeconds(0.4f);

        yield break;
    }


    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == InPlay_Collider)
        {
            Debug.Log("rock is in play");
            inPlay = true;
        }

        if (collider == boards_collider)
        {
            outOfPlay = true;
            inPlay = false;
            Debug.Log("trigger boards");
            inHouse = false;
            GetComponent<Rock_Info>().stopped = true;
            
        }

        if (collider == house_collider)
        {
            inHouse = true;
            Debug.Log("In House");
        }
    }

    void OnColliderEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Rock")
        {
            hit = true;
            Debug.Log("Hit!");
        }

        if (collider == boards_collider)
        {
            outOfPlay = true;
            inPlay = false;
            Debug.Log("collider boards");
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {

        //Check for a match with the specified name on any GameObject that collides with your GameObject
        if (collider == InPlay_Collider)
        {
            Debug.Log("Out");
            outOfPlay = true;
            inPlay = false;
        }

        if (collider == house_collider)
        {
            inHouse = false;
        }
    }

}
