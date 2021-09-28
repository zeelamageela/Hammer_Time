using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TigerForge;

public class MainMenu : MonoBehaviour
{
    public AudioManager am;
    public GameSettingsPersist gsp;
    EasyFileSave myFile;
    public GameObject contButton;

    void Start()
    {

        myFile = new EasyFileSave("my_game_data");

        if (myFile.Load())
            contButton.SetActive(true);
        else
            contButton.SetActive(false);
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
        SceneManager.LoadScene("Tourny_Menu_1");
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
