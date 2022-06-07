using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CrowdManager : MonoBehaviour
{
    public GameSettingsPersist gsp;
    public GameObject cameraMen;

    public int tournyType;
    public int crowdDensity;

    public GameObject crowdGO;
    public AnimatorOverrideController aoc;

    public GameObject[] crowdSections;

    public List<GameObject> activeCrowd;

    public AnimationClip[] crowdClips;

    Vector2 seatPos;
    // Start is called before the first frame update
    void Start()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();

        SetUpCrowd();
        if (tournyType > 0)
            cameraMen.SetActive(true);
        else
            cameraMen.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SetUpCrowd()
    {
        activeCrowd = new List<GameObject>();

        int counter = 0;

        for (int j = 0; j < 5; j++)
        {
            for (int i = 0; i < 19; i++)
            {
                seatPos = new Vector2((i * 0.46f), j * -0.55f);

                if (Random.Range(0, 10) < crowdDensity)
                {
                    if (i >= j)
                    {
                        counter++;
                        GameObject go = Instantiate(crowdGO, crowdSections[0].transform, false);
                        go.transform.localPosition = seatPos;
                        activeCrowd.Add(go);
                    }
                }
            }
        }

        

        foreach (GameObject go in activeCrowd)
        {
            Animator animator = go.GetComponent<Animator>();


            aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = aoc;
            int clipIndex = Random.Range(0, crowdClips.Length);
            
            aoc["Crowd1A"] = crowdClips[clipIndex];
            go.GetComponent<SpriteRenderer>().color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
        }
    }
    //Randomly list the crowd
    //Randomly colour them
}
