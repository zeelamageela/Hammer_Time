using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public AudioManager am;
    public GameSettingsPersist gsp;
   public void PlayGame()
    {
        gsp.loadGame = false;
        am = FindObjectOfType<AudioManager>();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        am.Play("Theme");
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
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
        gsp.loadGame = false;
        am = FindObjectOfType<AudioManager>();
        SceneManager.LoadScene("AIGame_2");
        am.Play("Theme");
    }

    public void Continue()
    {
        gsp.loadGame = true;
        am = FindObjectOfType<AudioManager>();
        SceneManager.LoadScene("AIGame_2");
        am.Play("Theme");
    }
}