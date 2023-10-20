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
    public GameObject beerGO;
    public AnimatorOverrideController aoc;

    public GameObject[] bgs;

    public GameObject[] benchSections;
    public GameObject[] rinkSections;
    public GameObject[] bleacherSections;
    public GameObject[] stadiumSections;
    public GameObject[] beersSnacksSections;
    public GameObject[] denSections;

    public List<GameObject> activeCrowd;

    public AnimationClip[] crowdClips;

    public Sprite[] beerSnacks;

    Vector2 seatPos;
    // Start is called before the first frame update
    void Start()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
        cm = FindObjectOfType<CareerManager>();

        crowdDensity = cm.currentTourny.crowdDensity;
        if (cm.currentTourny.BG == 0)
            ClubCrowd();
        else if (cm.currentTourny.BG == 1)
        {
            RinkCrowd();
        }
        else if (cm.currentTourny.BG == 2)
        {
            BleacherCrowd();
        }
        else if (cm.currentTourny.BG == 3)
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
        else if (cm.currentTourny.BG == 4)
        {
            OutdoorCrowd();
        }
        else if (cm.currentTourny.BG == 5)
        {
            DenCrowd();
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ClubCrowd()
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

        if (crowdDensity > 7)
        {
            for (int k = 0; k < 4; k++)
            {
                for (int i = 0; i < 4; i++)
                {
                    seatPos = new Vector2((i * 0.46f), 0f);

                    if (Random.Range(0, 10) < crowdDensity)
                    {
                        counter++;
                        GameObject go = Instantiate(crowdGO, benchSections[k].transform, false);
                        go.transform.localPosition = seatPos;
                        if (k % 2 == 0)
                            go.transform.localRotation = Quaternion.Euler(0f, 0f, -9f);
                        else
                            go.transform.localRotation = Quaternion.Euler(0f, 0f, 9f);
                        activeCrowd.Add(go);
                    }
                }
            }
        }

        for (int k = 4; k < 6; k++)
        {
            for (int i = 0; i < 11; i++)
            {
                seatPos = new Vector2((i * 0.46f), 0f);

                if (Random.Range(0, 10) < crowdDensity)
                {
                    counter++;
                    GameObject go = Instantiate(crowdGO, benchSections[k].transform, false);
                    go.transform.localPosition = seatPos;
                    activeCrowd.Add(go);
                }
            }
        }

        for (int j = 6; j < 8; j++)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 beerPos = new Vector2(i * 0.75f, Random.Range(0f, 0.05f));

                if (Random.Range(0, 12) < crowdDensity)
                {
                    GameObject go = Instantiate(beerGO, beersSnacksSections[j].transform, false);
                    go.transform.localPosition = beerPos;
                    int beerIndex = Random.Range(0, beerSnacks.Length);
                    beerGO.GetComponent<SpriteRenderer>().sprite = beerSnacks[beerIndex];
                }
            }
        }

        foreach (GameObject go in activeCrowd)
        {
            go.GetComponent<SpriteRenderer>().color = Random.ColorHSV(0.1f, 0.2f, 0.5f, 0.55f, 0.75f, 1f);
        }

        if (crowdDensity < 6)
            AssignAnimations("Clapping");
        else
            AssignAnimations("No Nuns");
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

        if (crowdDensity > 7)
        {
            for (int j = 0; j < 4; j++)
            {
                for (int i = 0; i < 4; i++)
                {
                    seatPos = new Vector2((i * 0.46f), 0f);

                    if (Random.Range(0, 10) < crowdDensity)
                    {
                        counter++;
                        GameObject go = Instantiate(crowdGO, rinkSections[j].transform, false);
                        go.transform.localPosition = seatPos;
                        activeCrowd.Add(go);
                    }
                }
            }
        }

        for (int j = 4; j < 6; j++)
        {
            for (int i = 0; i < 9; i++)
            {
                seatPos = new Vector2(i * 0.46f, 0f);

                if (Random.Range(0, 10) < crowdDensity)
                {
                    counter++;
                    GameObject go = Instantiate(crowdGO, rinkSections[j].transform, false);
                    go.transform.localPosition = seatPos;
                    activeCrowd.Add(go);
                }
            }
        }

        for (int i = 0; i < 25; i++)
        {
            seatPos = new Vector2(i * 0.46f, 0f);

            if (Random.Range(0, 10) < crowdDensity)
            {
                counter++;
                GameObject go = Instantiate(crowdGO, rinkSections[6].transform, false);
                go.transform.localPosition = seatPos;
                activeCrowd.Add(go);
            }
        }

        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 beerPos = new Vector2(i * 0.75f, Random.Range(0f, 0.05f));

                if (Random.Range(0, 15) < crowdDensity)
                {
                    GameObject go = Instantiate(beerGO, beersSnacksSections[j].transform, false);
                    go.transform.localPosition = beerPos;
                    int beerIndex = Random.Range(0, beerSnacks.Length);
                    beerGO.GetComponent<SpriteRenderer>().sprite = beerSnacks[beerIndex];
                }
            }
        }

        for (int i = 0; i < 15; i++)
        {
            Vector2 beerPos = new Vector2(i * 0.75f, Random.Range(0f, 0.05f));

            if (Random.Range(0, 15) < crowdDensity)
            {
                GameObject go = Instantiate(beerGO, beersSnacksSections[2].transform, false);
                go.transform.localPosition = beerPos;
                int beerIndex = Random.Range(0, beerSnacks.Length);
                beerGO.GetComponent<SpriteRenderer>().sprite = beerSnacks[beerIndex];
            }
        }

        foreach (GameObject go in activeCrowd)
        {
            go.GetComponent<SpriteRenderer>().color = Random.ColorHSV(0f, 0.5f, 0.6f, 0.65f, 0.75f, 1f);
        }

        if (crowdDensity < 6)
            AssignAnimations("Clapping");
        else
            AssignAnimations("No Nuns");
    }

    void BleacherCrowd()
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

        activeCrowd = new List<GameObject>();

        int counter = 0;

        for (int j = 0; j < 5; j++)
        {
            for (int i = 0; i < 15; i++)
            {
                seatPos = new Vector2((i * 0.46f), j * -0.55f);

                if (Random.Range(0, 10) < crowdDensity)
                {
                    counter++;
                    GameObject go = Instantiate(crowdGO, bleacherSections[0].transform, false);
                    go.transform.localPosition = seatPos;
                    go.GetComponent<SpriteRenderer>().sortingOrder = j + 6;
                    activeCrowd.Add(go);
                }
            }
        }

        for (int k = 1; k < 9; k++)
        {
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 15; i++)
                {
                    if (k % 2 == 0)
                        seatPos = new Vector2((i * 0.46f) + (j * 0.1f), j * -0.55f);
                    else
                        seatPos = new Vector2((i * 0.46f) - (j * 0.1f), j * -0.55f);

                    if (Random.Range(0, 10) < crowdDensity)
                    {
                        counter++;
                        GameObject go = Instantiate(crowdGO, bleacherSections[k].transform, false);
                        go.transform.localPosition = seatPos;
                        go.GetComponent<SpriteRenderer>().sortingOrder = j + 3;
                        activeCrowd.Add(go);
                    }
                }
            }
        }

        for (int j = 9; j < 11; j++)
        {
            for (int i = 0; i < 11; i++)
            {
                seatPos = new Vector2(i * 0.46f, 0f);

                if (Random.Range(0, 10) < crowdDensity)
                {
                    counter++;
                    GameObject go = Instantiate(crowdGO, bleacherSections[j].transform, false);
                    go.transform.localPosition = seatPos;
                    activeCrowd.Add(go);
                }
            }
        }

        for (int j = 11; j < 13; j++)
        {
            for (int i = 0; i < 14; i++)
            {
                seatPos = new Vector2(i * 0.46f, 0f);

                if (Random.Range(0, 10) < crowdDensity)
                {
                    counter++;
                    GameObject go = Instantiate(crowdGO, bleacherSections[j].transform, false);
                    go.transform.localPosition = seatPos;
                    activeCrowd.Add(go);
                }
            }
        }

        for (int j = 3; j < 5; j++)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 beerPos = new Vector2(i * 0.75f, Random.Range(0f, 0.05f));

                if (Random.Range(0, 15) < crowdDensity)
                {
                    GameObject go = Instantiate(beerGO, beersSnacksSections[j].transform, false);
                    go.transform.localPosition = beerPos;
                    int beerIndex = Random.Range(0, beerSnacks.Length);
                    beerGO.GetComponent<SpriteRenderer>().sprite = beerSnacks[beerIndex];
                }
            }
        }

        for (int i = 0; i < 15; i++)
        {
            Vector2 beerPos = new Vector2(i * 1f, Random.Range(0f, 0.05f));

            if (Random.Range(0, 15) < crowdDensity)
            {
                GameObject go = Instantiate(beerGO, beersSnacksSections[5].transform, false);
                go.transform.localPosition = beerPos;
                int beerIndex = Random.Range(0, beerSnacks.Length);
                beerGO.GetComponent<SpriteRenderer>().sprite = beerSnacks[beerIndex];
            }
        }

        foreach (GameObject go in activeCrowd)
        {
            go.GetComponent<SpriteRenderer>().color = Random.ColorHSV(0.25f, 0.45f, 0.25f, 1f, 0.85f, 1f);
        }

        if (crowdDensity < 7)
            AssignAnimations("Clapping");
        else
            AssignAnimations("No Nuns");
    }

    void StadiumCrowd(bool left)
    {
        activeCrowd = new List<GameObject>();
        if (left)
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
                if (i == 4)
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

        foreach (GameObject go in activeCrowd)
        {
            go.GetComponent<SpriteRenderer>().color = Random.ColorHSV(0f, 1f, 0.25f, 1f, 0.5f, 1f);
        }

        AssignAnimations("No Nuns");
    }

    void OutdoorCrowd()
    {
        for (int i = 0; i < bgs.Length; i++)
        {
            if (i == 5)
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
                seatPos = new Vector2((i * 0.46f), j * -0.55f);

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

        for (int k = 1; k < 9; k++)
        {
            for (int j = 0; j < 5; j++)
            {
                for (int i = 0; i < 15; i++)
                {
                    if (k % 2 == 0)
                        seatPos = new Vector2((i * 0.46f) + (j * 0.1f), j * -0.55f);
                    else
                        seatPos = new Vector2((i * 0.46f) - (j * 0.1f), j * -0.55f);

                    if (Random.Range(0, 10) < crowdDensity)
                    {
                        counter++;
                        GameObject go = Instantiate(crowdGO, bleacherSections[k].transform, false);
                        go.transform.localPosition = seatPos;
                        go.GetComponent<SpriteRenderer>().sortingOrder = j + 3;
                        activeCrowd.Add(go);
                    }
                }
            }
        }

        foreach (GameObject go in activeCrowd)
        {
            go.GetComponent<SpriteRenderer>().color = Random.ColorHSV(0.55f, 0.65f, 0.25f, 1f, 0.85f, 1f);
        }

        AssignAnimations("No Nuns");
    }

    void DenCrowd()
    {
        for (int i = 0; i < bgs.Length; i++)
        {
            if (i == 6)
            {
                bgs[i].SetActive(true);
            }
            else
                bgs[i].SetActive(false);

        }

        activeCrowd = new List<GameObject>();

        int counter = 0;

        for (int k = 0; k < 4; k++)
        {
            for (int i = 0; i < 5; i++)
            {
                seatPos = new Vector2(i * 0.6f, 0f);

                if (Random.Range(0, 10) < crowdDensity)
                {
                    counter++;
                    GameObject go = Instantiate(crowdGO, denSections[k].transform, false);
                    go.transform.localPosition = seatPos;
                    go.GetComponent<SpriteRenderer>().sortingOrder = 3;
                    activeCrowd.Add(go);
                }
            }
        }

        for (int k = 4; k < 7; k++)
        {
            for (int i = 0; i < 4; i++)
            {
                seatPos = new Vector2(i * 0.6f, 0f);

                if (Random.Range(0, 10) < crowdDensity)
                {
                    counter++;
                    GameObject go = Instantiate(crowdGO, denSections[k].transform, false);
                    go.transform.localPosition = seatPos;
                    go.GetComponent<SpriteRenderer>().sortingOrder = 3;
                    activeCrowd.Add(go);
                }
            }
        }

        for (int j = 7; j < 12; j++)
        {
            for (int i = 0; i < 10; i++)
            {
                if (i < 5)
                    seatPos = new Vector2(i * 0.5f, 0f);
                else
                    seatPos = new Vector2(5.25f * 0.5f, (i - 3.5f) * -0.5f);

                if (Random.Range(0, 10) < crowdDensity)
                {
                    counter++;
                    GameObject go = Instantiate(crowdGO, denSections[j].transform, false);
                    go.transform.localPosition = seatPos;
                    if (i > 4)
                        go.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
                    activeCrowd.Add(go);
                }
            }
        }

        for (int j = 12; j < 14; j++)
        {
            for (int i = 0; i < 20; i++)
            {
                if (i < 5)
                    seatPos = new Vector2(i * 0.5f, 0f);
                else if (i < 10)
                    seatPos = new Vector2(5.25f * 0.5f, (i - 3.5f) * 0.5f);
                else if (i < 15)
                    seatPos = new Vector2(5.25f * 0.5f, (i - 1.5f) * 0.5f);
                else
                    seatPos = new Vector2(5.25f * 0.5f, (i + 0.5f) * 0.5f);

                if (Random.Range(0, 10) < crowdDensity)
                {
                    counter++;
                    GameObject go = Instantiate(crowdGO, denSections[j].transform, false);
                    go.transform.localPosition = seatPos;
                    if (i > 4)
                        go.transform.localRotation = Quaternion.Euler(0f, 0f, -90f);
                    else
                        go.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);

                    activeCrowd.Add(go);
                }
            }
        }

        for (int i = 0; i < 7; i++)
        {
            seatPos = new Vector2(i * 1.125f, 0f);

            if (Random.Range(0, 10) < crowdDensity)
            {
                counter++;
                GameObject go = Instantiate(crowdGO, denSections[14].transform, false);
                go.transform.localPosition = seatPos;
                if (i == 0 | i == 2 | i == 5)
                    go.transform.localRotation = Quaternion.Euler(0f, 0f, 22f);
                else
                    go.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);
                activeCrowd.Add(go);
            }
        }

        for (int i = 0; i < 5; i++)
        {
            seatPos = new Vector2(i * 1.125f, 0f);

            if (Random.Range(0, 10) < crowdDensity)
            {
                counter++;
                GameObject go = Instantiate(crowdGO, denSections[15].transform, false);
                go.transform.localPosition = seatPos;
                if (i == 0 | i == 3)
                    go.transform.localRotation = Quaternion.Euler(0f, 0f, 22f);
                else
                    go.transform.localRotation = Quaternion.Euler(0f, 0f, -25f);
                activeCrowd.Add(go);
            }
        }

        foreach (GameObject go in activeCrowd)
        {
            go.GetComponent<SpriteRenderer>().color = Random.ColorHSV(0.85f, 0.95f, 0.75f, 1f, 0.5f, 1f);
        }

        if (crowdDensity < 7)
            AssignAnimations("Clapping");
        else
            AssignAnimations("No Nuns");
    }

    void AssignAnimations(string range)
    {
        AnimationClip[] tempCrowdClips = new AnimationClip[crowdClips.Length];
        switch (range)
        {
            case "Clapping":
                tempCrowdClips = new AnimationClip[20];
                for (int i = 0; i < tempCrowdClips.Length; i++)
                {
                    tempCrowdClips[i] = crowdClips[i];
                }
                break;

            case "Rowdy":
                tempCrowdClips = new AnimationClip[13];
                for (int i = 16; i < tempCrowdClips.Length; i++)
                {
                    tempCrowdClips[i] = crowdClips[i];
                }
                break;

            case "Nuns":
                tempCrowdClips = new AnimationClip[1];
                for (int i = 29; i < tempCrowdClips.Length; i++)
                {
                    tempCrowdClips[i] = crowdClips[i];
                }
                break;

            case "Beer Guys":
                tempCrowdClips = new AnimationClip[1];
                for (int i = 28; i < tempCrowdClips.Length; i++)
                {
                    tempCrowdClips[i] = crowdClips[i];
                }
                break;

            case "No Nuns":
                tempCrowdClips = new AnimationClip[29];
                for (int i = 0; i < tempCrowdClips.Length; i++)
                {
                    tempCrowdClips[i] = crowdClips[i];
                }
                break;

            default:
                for (int i = 0; i < tempCrowdClips.Length; i++)
                {
                    tempCrowdClips[i] = crowdClips[i];
                }
                break;
        }

        foreach (GameObject go in activeCrowd)
        {
            Animator animator = go.GetComponent<Animator>();
            

            aoc = new AnimatorOverrideController(animator.runtimeAnimatorController);
            animator.runtimeAnimatorController = aoc;
            int clipIndex = Random.Range(0, tempCrowdClips.Length);

            aoc["Crowd1A"] = tempCrowdClips[clipIndex];

            //float randomIdleStart = Random.Range(0, animator.GetCurrentAnimatorStateInfo(0).length); //Set a random part of the animation to start from
            //string randomName = aoc["Crowd1A"].name;
            //animator.Play(randomName, 0, randomIdleStart);
        }

        
    }
}
