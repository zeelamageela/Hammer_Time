using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CrowdManager : MonoBehaviour
{
    public CareerManager cm;
    public GameSettingsPersist gsp;
    public BGManager bgm;

    public GameObject cameraMen;

    public int tournyType;
    public int crowdDensity;

    public GameObject crowdGO;
    public AnimatorOverrideController aoc;

    public GameObject[] bgs;

    public GameObject[] benchSections;
    public GameObject[] bleacherSections;
    public GameObject[] stadiumSections;

    public List<GameObject> activeCrowd;

    public AnimationClip[] crowdClips;

    Vector2 seatPos;
    // Start is called before the first frame update
    void Start()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
        cm = FindObjectOfType<CareerManager>();

        crowdDensity = cm.currentTourny.crowdDensity;
        if (cm.currentTourny.BG == 0 | cm.currentTourny.BG == 4)
            LocalCrowd();
        else if (cm.currentTourny.BG == 1)
        {
            RinkCrowd();
        }
        else if (cm.currentTourny.BG == 2 | cm.currentTourny.BG == 3)
        {
            if (Random.Range(0f, 1f) < 0.5f)
                StadiumCrowd(false);
            else
                StadiumCrowd(true);

            if (crowdDensity > 5)
                cameraMen.SetActive(true);
            else
                cameraMen.SetActive(false);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void LocalCrowd()
    {
        for (int i = 0; i < bgs.Length; i++)
        {
            if (i == 0)
            {
                bgs[i].SetActive(true);
            }
            else
                bgs[i].SetActive(false);

        }

        activeCrowd = new List<GameObject>();

        int counter = 0;

        for (int k = 0; k < benchSections.Length; k++)
        {
            for (int i = 0; i < 15; i++)
            {
                seatPos = new Vector2((i * 0.46f), 0f);

                if (Random.Range(0, 10) < crowdDensity)
                {
                    counter++;
                    GameObject go = Instantiate(crowdGO, benchSections[k].transform, false);
                    go.transform.localPosition = seatPos;
                    if (k % 2 == 0)
                        go.transform.localRotation = Quaternion.Euler(0f, 0f, -15f);
                    else
                        go.transform.localRotation = Quaternion.Euler(0f, 0f, 15f);
                    activeCrowd.Add(go);
                }
            }
        }

        AssignAnimations();
    }

    void StadiumCrowd(bool left)
    {
        activeCrowd = new List<GameObject>();
        if (left)
        {
            for (int i = 0; i < bgs.Length; i++)
            {
                if (i == 2)
                {
                    bgs[i].SetActive(true);
                }
                else
                    bgs[i].SetActive(false);

            }

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
                            GameObject go = Instantiate(crowdGO, stadiumSections[0].transform, false);
                            go.transform.localPosition = seatPos;
                            go.GetComponent<SpriteRenderer>().sortingOrder = j + 3;
                            activeCrowd.Add(go);
                        }
                    }
                }
            }

            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 19; i++)
                {
                    seatPos = new Vector2((i * -0.46f), j * -0.55f);

                    if (Random.Range(0, 10) < crowdDensity)
                    {
                        if (i >= j)
                        {
                            counter++;
                            GameObject go = Instantiate(crowdGO, stadiumSections[1].transform, false);
                            go.transform.localPosition = seatPos;
                            go.GetComponent<SpriteRenderer>().sortingOrder = j + 3;
                            activeCrowd.Add(go);
                        }
                    }
                }
            }

            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 51; i++)
                {
                    seatPos = new Vector2((i * 0.46f), j * -0.55f);

                    if (Random.Range(0, 10) < crowdDensity)
                    {
                        counter++;
                        GameObject go = Instantiate(crowdGO, stadiumSections[2].transform, false);
                        go.transform.localPosition = seatPos;
                        go.GetComponent<SpriteRenderer>().sortingOrder = j + 3;
                        activeCrowd.Add(go);
                    }
                }
            }

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
                            GameObject go = Instantiate(crowdGO, stadiumSections[3].transform, false);
                            go.transform.localPosition = seatPos;
                            go.GetComponent<SpriteRenderer>().sortingOrder = j + 3;
                            activeCrowd.Add(go);
                        }
                    }
                }
            }

        }
        else
        {
            for (int i = 0; i < bgs.Length; i++)
            {
                if (i == 3)
                {
                    bgs[i].SetActive(true);
                }
                else
                    bgs[i].SetActive(false);

            }

            int counter = 0;

            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 19; i++)
                {
                    seatPos = new Vector2((i * -0.46f), j * -0.55f);

                    if (Random.Range(0, 10) < crowdDensity)
                    {
                        if (i >= j)
                        {
                            counter++;
                            GameObject go = Instantiate(crowdGO, stadiumSections[4].transform, false);
                            go.transform.localPosition = seatPos;
                            go.GetComponent<SpriteRenderer>().sortingOrder = j + 3;
                            activeCrowd.Add(go);
                        }
                    }
                }
            }

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
                            GameObject go = Instantiate(crowdGO, stadiumSections[5].transform, false);
                            go.transform.localPosition = seatPos;
                            go.GetComponent<SpriteRenderer>().sortingOrder = j + 3;
                            activeCrowd.Add(go);
                        }
                    }
                }
            }

            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 51; i++)
                {
                    seatPos = new Vector2((i * -0.46f), j * -0.55f);

                    if (Random.Range(0, 10) < crowdDensity)
                    {
                        counter++;
                        GameObject go = Instantiate(crowdGO, stadiumSections[6].transform, false);
                        go.transform.localPosition = seatPos;
                        go.GetComponent<SpriteRenderer>().sortingOrder = j + 3;
                        activeCrowd.Add(go);
                    }
                }
            }

            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 19; i++)
                {
                    seatPos = new Vector2((i * -0.46f), j * -0.55f);

                    if (Random.Range(0, 10) < crowdDensity)
                    {
                        if (i >= j)
                        {
                            counter++;
                            GameObject go = Instantiate(crowdGO, stadiumSections[7].transform, false);
                            go.transform.localPosition = seatPos;
                            go.GetComponent<SpriteRenderer>().sortingOrder = j + 3;
                            activeCrowd.Add(go);
                        }
                    }
                }
            }
        }


        AssignAnimations();
    }

    void RinkCrowd()
    {
        for (int i = 0; i < bgs.Length; i++)
        {
            if (i == 1)
            {
                bgs[i].SetActive(true);
            }
            else
                bgs[i].SetActive(false);

        }

        activeCrowd = new List<GameObject>();

        int counter = 0;

        for (int j = 0; j < 5; j++)
        {
            for (int i = 0; i < 15; i++)
            {
                seatPos = new Vector2((i * 0.46f), j * 0.55f);

                if (Random.Range(0, 10) < crowdDensity)
                {
                    counter++;
                    GameObject go = Instantiate(crowdGO, bleacherSections[0].transform, false);
                    go.transform.localPosition = seatPos;
                    go.GetComponent<SpriteRenderer>().sortingOrder = j + 3;
                    activeCrowd.Add(go);
                }
            }
        }

        for (int i = 0; i < 15; i++)
        {
            seatPos = new Vector2(i * 0.46f, 0f);

            if (Random.Range(0, 10) < crowdDensity)
            {
                counter++;
                GameObject go = Instantiate(crowdGO, bleacherSections[1].transform, false);
                go.transform.localPosition = seatPos;
                go.transform.localRotation = Quaternion.Euler(0f, 0f, -15f);
                activeCrowd.Add(go);
            }
        }

        for (int i = 0; i < 15; i++)
        {
            seatPos = new Vector2(i * 0.46f, 0f);

            if (Random.Range(0, 10) < crowdDensity)
            {
                counter++;
                GameObject go = Instantiate(crowdGO, bleacherSections[2].transform, false);
                go.transform.localPosition = seatPos;
                go.transform.localRotation = Quaternion.Euler(0f, 0f, 15f);
                activeCrowd.Add(go);
            }
        }

        AssignAnimations();
    }

    void AssignAnimations()
    {
        foreach (GameObject go in activeCrowd)
        {
            Animator animator = go.GetComponent<Animator>();


            aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = aoc;
            int clipIndex = Random.Range(0, crowdClips.Length);

            aoc["Crowd1A"] = crowdClips[clipIndex];
            go.GetComponent<SpriteRenderer>().color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 0.8f);
        }
    }
}
