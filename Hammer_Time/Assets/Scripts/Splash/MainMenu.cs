using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TigerForge;

public class MainMenu : MonoBehaviour
{
    public AudioManager am;
    public GameSettingsPersist gsp;
    EasyFileSave myFile;
    public GameObject contButton;

    public Text allTimeEarnings;
    public Text allTimeNames;

    public Image[] trophyImgList;
    public bool[] trophyWon;

    void Start()
    {

        myFile = new EasyFileSave("my_hiscore_data");

        float[] allTimeEarningsList;
        string[] allTimeNamesList;
        trophyWon = new bool[trophyImgList.Length];


        if (myFile.Load())
        {
            Debug.Log("All Time Load");
            allTimeEarningsList = myFile.GetArray<float>("All Time Earnings");
            allTimeNamesList = myFile.GetArray<string>("All Time Names");
            trophyWon = myFile.GetArray<bool>("All Time Trophies Won");
            allTimeEarnings.text = "$" + allTimeEarningsList[0].ToString("n0");
            allTimeNames.text = allTimeNamesList[0];

            myFile.Dispose();
        }
        else
        {
            Debug.Log("No Hi Score data");
            allTimeEarningsList = new float[1];
            allTimeNamesList = new string[1];
            allTimeEarnings.gameObject.SetActive(false);

            allTimeNames.text = "No High Score Set";

            myFile.Dispose();
        }

        for (int i = 0; i < trophyWon.Length; i++)
        {
            if (trophyWon[i])
                trophyImgList[i].color += new Color(0f, 0f, 0f, 0.5f);
            else

                trophyImgList[i].color -= new Color(0f, 0f, 0f, 0.5f);

        }


        am.PlayBG(0);
        //if (myFile.Load())
        //    contButton.SetActive(true);
        //else
        //    contButton.SetActive(false);
    }
   public void PlayGame()
    {
        am = FindObjectOfType<AudioManager>();
        SceneManager.LoadScene("AIGame");
        am.Play("Theme");
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void Tournament()
    {
        SceneManager.LoadScene("Career_Menu");
    }
    public void Tutorial()
    {
        gsp.tutorial = true;
        am = FindObjectOfType<AudioManager>();
        SceneManager.LoadScene("Tutorial_1");
        am.Play("Theme");
    }

    public void AIGame()
    {
        am = FindObjectOfType<AudioManager>();
        SceneManager.LoadScene("AIGame_2");
        am.Play("Theme");
    }

    public void Continue()
    {
        gsp.loadGame = true;
        Debug.Log("Load Game is " + gsp.loadGame);
        am = FindObjectOfType<AudioManager>();
        SceneManager.LoadScene("AIGame");
        am.Play("Theme");
    }
}
