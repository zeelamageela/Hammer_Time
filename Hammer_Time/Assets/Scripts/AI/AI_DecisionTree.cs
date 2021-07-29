using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_DecisionTree : MonoBehaviour
{
    public GameManager gm;
    public AIManager aim;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnShot (int rockCurrent)
    {

    }

    IEnumerator ShotSelectHammer(int rockCurrent)
    {
        switch (rockCurrent)
        {
            case 0:

                //randomly choose
                //if (Random.value > 0.5f)
                //{
                //    StartCoroutine(aim.DrawFourFoot(rockCurrent));
                //}
                //else StartCoroutine(aim.OnShot("Tight Centre Guard"));
                //break;

            case 2:
                yield break;
        }
    }
}
