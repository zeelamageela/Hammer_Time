using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TournySelector : MonoBehaviour
{
    CareerManager cm;

    public Text weekText;
    public Tourny[] tournies;

    public Tourny[] tour;
    public Tourny[] provQual;
    public Tourny tourChampionship;
    public Tourny provChampionship;

    public Tourny[] activeTournies;

    public Tourny emptyTourny;
    public Tourny currentTourny;

    public TournyPanel[] panels;

    int week;

    // Start is called before the first frame update
    void Start()
    {
        cm = FindObjectOfType<CareerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Shuffle(Tourny[] a)
    {
        // Loops through array
        for (int i = a.Length - 1; i > 0; i--)
        {
            // Randomize a number between 0 and i (so that the range decreases each time)
            int rnd = Random.Range(0, i);

            // Save the value of the current i, otherwise it'll overright when we swap the values
            Tourny temp = a[i];

            // Swap the new and old values
            a[i] = a[rnd];
            a[rnd] = temp;
        }

        // Print
        //PrintRows(a);
        //for (int i = 0; i < a.Length; i++)
        //{
        //	Print;
        //}
    }

    public void SetActiveTournies()
    {
        weekText.text = "Week " + cm.week.ToString();
        Shuffle(tournies);

        if (Random.Range(0f, 1f) > 0.5f)
        {
            Shuffle(provQual);
            for (int i = 0; i < provQual.Length; i++)
            {
                if (provQual[i].complete != true)
                {
                    activeTournies[0] = provQual[i];
                    provQual[i].complete = true;
                    break;
                }
            }
        }
        else
        {
            Shuffle(tour);
            for (int i = 0; i < tour.Length; i++)
            {
                if (tour[i].complete != true)
                {
                    activeTournies[0] = tour[i];
                    tour[i].complete = true;
                    break;
                }
            }
        }

        if (activeTournies[0].tour)
        {

            Shuffle(provQual);
            for (int i = 0; i < provQual.Length; i++)
            {
                if (provQual[i].complete != true)
                {
                    activeTournies[1] = provQual[i];
                    provQual[i].complete = true;
                    break;
                }
            }
        }
        else
        {
            Shuffle(tour);
            if (Random.Range(0f, 1f) > 0.5f)
            {
                for (int i = 0; i < tour.Length; i++)
                {
                    if (tour[i].complete != true)
                    {
                        activeTournies[1] = tour[i];
                        tour[i].complete = true;
                        break;
                    }
                }
            }
            else
            {
                Shuffle(provQual);
                for (int i = 0; i < provQual.Length; i++)
                {
                    if (provQual[i].complete != true)
                    {
                        activeTournies[1] = provQual[i];
                        provQual[i].complete = true;
                        break;
                    }
                }
            }
        }

        if (cm.tourQual)
            activeTournies[2] = tourChampionship;
        else if (cm.provQual)
            activeTournies[2] = provChampionship;
        else
            activeTournies[2] = emptyTourny;


        SetPanels();

    }

    public void SetPanels()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].location.text = activeTournies[i].location;
            panels[i].buttonText.text = activeTournies[i].name;
            panels[i].teams.text = activeTournies[i].teams.ToString();
            panels[i].format.text = activeTournies[i].format;
            panels[i].purse.text = "$" + activeTournies[i].prizeMoney.ToString();
            panels[i].entry.text = "$" + activeTournies[i].entryFee.ToString();

        }
    }

    public void SelectTourny(int button)
    {
        currentTourny = activeTournies[button];

        
    }
}
