using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TigerForge;

public class ContinueMenu : MonoBehaviour
{
    public AudioManager am;
    public GameSettingsPersist gsp;

    public GameObject redSpinner;
    public GameObject yellowSpinner;

    EasyFileSave myFile;
    public Text scoreDisplay;
    public Text endDisplay;
    public Text shotDisplay;

    void OnEnable()
    {
        //GameSettingsPersist gsp = GameObject.Find("GameSettingsPersist").GetComponent<GameSettingsPersist>();

        myFile = new EasyFileSave("my_game_data");

        if (myFile.Load())
        {
            scoreDisplay.text = myFile.GetInt("Red Score") + " - " + myFile.GetInt("Yellow Score");
            endDisplay.text = myFile.GetInt("Current End") + " / " + myFile.GetInt("End Total");
            shotDisplay.text = (myFile.GetInt("Current Rock") + 1) + " / " + (myFile.GetInt("Rocks Per Team") * 2);

            //redSpinner.SetActive(true);
            //yellowSpinner.SetActive(false);

            if (myFile.GetInt("Current Rock") % 2 == 0)
            {
                if (myFile.GetBool("Red Hammer"))
                {
                    redSpinner.SetActive(false);
                    yellowSpinner.SetActive(true);
                }
                else
                {
                    redSpinner.SetActive(true);
                    yellowSpinner.SetActive(false);
                }
            }
            else
            {
                {
                    if (myFile.GetBool("Red Hammer"))
                    {
                        redSpinner.SetActive(true);
                        yellowSpinner.SetActive(false);
                    }
                    else
                    {
                        redSpinner.SetActive(false);
                        yellowSpinner.SetActive(true);
                    }
                }
            }
        }

        
        //gsp.ends = myFile.GetInt("End Total");
        //gsp.endCurrent = myFile.GetInt("End Current");
        //gsp.rocks = myFile.GetInt("Rock Total");
        //gsp.rockCurrent = myFile.GetInt("Rock Current");
        //gsp.redHammer = myFile.GetBool("Red Hammer");
        //gsp.aiYellow = myFile.GetBool("Ai Yellow");
        //gsp.yellowScore = myFile.GetInt("Yellow Score");
        //gsp.redScore = myFile.GetInt("Red Score");
        //gsp.loadGame = true;
    }
    public void Continue()
    {
        //Debug.Log("Load Game is " + gsp.loadGame);
        am = FindObjectOfType<AudioManager>();
        SceneManager.LoadScene("AIGame_2");
        am.Play("Theme");
    }

}
