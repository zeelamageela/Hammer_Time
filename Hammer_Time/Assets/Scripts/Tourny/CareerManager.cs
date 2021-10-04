using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TigerForge;

public class CareerManager : MonoBehaviour
{
    public static CareerManager instance;
    GameSettingsPersist gsp;
    TournyManager tm;
    PlayoffManager pm;
    TournySettings ts;
    TournySelector tSel;
    EasyFileSave myFile;

    public int week;
    public int seasonLength;
    public string teamName;
    public int money;
    public Vector2Int record;
    public bool provQual;
    public bool tourQual;
    public Vector2Int tourRecord;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
        tm = FindObjectOfType<TournyManager>();
        pm = FindObjectOfType<PlayoffManager>();
        ts = FindObjectOfType<TournySettings>();
        tSel = FindObjectOfType<TournySelector>();
    }

    void LoadCareer()
    {
        myFile = new EasyFileSave("my_career_data");

        if (myFile.Load())
        {
            myFile.GetInt("Week", week);
            myFile.GetString("Team Name", teamName);
            myFile.GetUnityVector2("Record", record);
            myFile.GetFloat("Money", money);
            myFile.GetBool("Prov Qual", provQual);
            myFile.GetBool("Tour Qual", tourQual);
            myFile.GetUnityVector2("Tour Record", tourRecord);
        }
    }

    public void GameResults(bool win, bool loss)
    {
        if (win)
        {
            record += new Vector2Int(1, 0);
            if (tSel.currentTourny.tour)
            {
                tourRecord += new Vector2Int(1, 0);
            }
        }
        else if (loss)
        {
            record += new Vector2Int(0, 1);
            if (tSel.currentTourny.tour)
            {
                tourRecord += new Vector2Int(0, 1);
            }
        }
            
    }

    public void NextWeek()
    {
        week++;
        tSel.SetActiveTournies();
    }
    public void PlayTourny()
    {

    }


}
