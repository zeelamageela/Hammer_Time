using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock_Colliders : MonoBehaviour
{
    public Collider2D InPlay_Collider;
    public Collider2D boards_collider;
    public Collider2D house_collider;
    public Collider2D launchCollider;

    private Rigidbody2D body;

    public bool outOfPlay = false;
    public bool inPlay = false;
    public bool hit = false;
    public bool inHouse = false;
    public bool shotTaken = false;

    AudioManager am;
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

        GameObject launch = GameObject.Find("Launcher");
        launchCollider = launch.GetComponent<Collider2D>();
        launchCollider.enabled = false;

        am = FindObjectOfType<AudioManager>();
    }

// Update is called once per frame
    void Update()
    {
        if (outOfPlay)
        {
            StartCoroutine(OutOfPlay());
        }
    }

    IEnumerator OutOfPlay()
    {
        outOfPlay = true;
        am.Play("OUtOfPlay");
        body.velocity = Vector2.zero;
        body.angularVelocity = 0f;
        GetComponent<Collider2D>().enabled = false;

        GetComponent<Rock_Info>().stopped = true;
        GetComponent<Rock_Info>().rest = true;

        yield return new WaitForSeconds(0.5f);

        gameObject.SetActive(false);
    }


    void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider == launchCollider)
        {
            Debug.Log("Shot is taken");
            shotTaken = true;
            launchCollider.enabled = false;
            Debug.Log(GetComponent<Rigidbody2D>().velocity.x + ", " + GetComponent<Rigidbody2D>().velocity.y);
        }
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
        }

        if (collider == house_collider)
        {
            inHouse = true;
            Debug.Log("In House");
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Rock")
        {
            hit = true;
            Debug.Log("Hit!");
            am.Play("Hit");
        }

        if (collision.gameObject.tag == "Boards")
        {
            outOfPlay = true;
            inPlay = false;
            Debug.Log("collider boards");
            StartCoroutine(OutOfPlay());
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if (collider == InPlay_Collider)
        {
            Debug.Log("Out");
            inPlay = false;
            inHouse = false;
            StartCoroutine(OutOfPlay());
        }

        if (collider == house_collider)
        {
            Debug.Log("Out of House");
            inHouse = false;
        }
    }

}
