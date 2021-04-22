using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndManager : MonoBehaviour
{
    //int red_score;
    public int rock_count = 2;
    public GameObject redShooter;
    public GameObject yellowShooter;

    GameObject yellowRock;
    GameObject redRock;

    bool redReleased;
    bool yellowReleased;

    //public bool yellowTurn;
    //public bool redTurn;
    
    public bool activeRock;

    // Start is called before the first frame update
    void Start()
    {
        //activeRock = redShooter.GetComponent<Rock_InPlay>().activeRock;
        Debug.Log("activeRock izz " + activeRock);
        //Instantiate(redRock, new Vector2(0f, -19.49f), Quaternion.identity);
        yellowShooter.SetActive(false);
        redShooter.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        redReleased = redShooter.GetComponent<Rock_Release>().released;
        yellowReleased = yellowShooter.GetComponent<Rock_Release>().released;

        if (activeRock != true)
        {

            if(rock_count%2 == 0)
            {
                yellowShooter.SetActive(true);
                activeRock = true;
                Debug.Log(rock_count + " rocks left.");
            }
            if(rock_count%2 == 1)
            {
                redShooter.SetActive(true);
                activeRock = true;
                Debug.Log(rock_count + " rocks left.");
                redRock = GameObject.Find("Red_Rock");
            }

        }

        if (yellowReleased)
        {
            yellowRock = GameObject.FindWithTag("Rock");
            Debug.Log("Yellow is released");

            if (yellowRock.transform.hasChanged)
            {
                yellowShooter.transform.hasChanged = false;

                Debug.Log("Yellow is moving");
            }
            else
            {
                activeRock = false;
                rock_count--;
                Debug.Log("Yellow is stopped");
                return;
            }
        }

        if (redReleased)
        {
            redRock = GameObject.Find("Red_Rock");

            if (redRock.transform.hasChanged)
            {
                redRock.transform.hasChanged = false;

                Debug.Log("Red is moving");
            }
            else
            {
                activeRock = false;
                rock_count--;
                Debug.Log("Red is stopped");
            }
            

        }
            
}


    }


 

