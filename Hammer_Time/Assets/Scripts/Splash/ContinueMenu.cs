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

    public EasyFileSave myFile;
    public Text scoreDisplay;
    public Text endDisplay;
    public Text shotDisplay;

    void OnEnable()
    {
        myFile = new EasyFileSave("my_game_data");

        myFile.Load();
        scoreDisplay.text = myFile.GetInt("Red Score") + " - " + myFile.GetInt("Yellow Score");
        endDisplay.text = myFile.GetInt("Current End") + " / " + myFile.GetInt("End Total");
        shotDisplay.text = myFile.GetInt("Current Rock") + " / " + myFile.GetInt("Rock Total");

        
        gsp.ends = myFile.GetInt("End Total");
        gsp.endCurrent = myFile.GetInt("End Current");
        gsp.rocks = myFile.GetInt("Rock Total");
        gsp.rockCurrent = myFile.GetInt("Rock Current");
        gsp.redHammer = myFile.GetBool("Red Hammer");
        gsp.aiYellow = myFile.GetBool("Ai Yellow");
        gsp.yellowScore = myFile.GetInt("Yellow Score");
        gsp.redScore = myFile.GetInt("Red Score");
    }
    public void Continue()
    {
        gsp.loadGame = true;
        Debug.Log("Load Game is " + gsp.loadGame);
        am = FindObjectOfType<AudioManager>();
        SceneManager.LoadScene("AIGame_2");
        am.Play("Theme");
    }

}
