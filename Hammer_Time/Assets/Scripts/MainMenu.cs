using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public AudioManager am;

   public void PlayGame()
    {
        am = FindObjectOfType<AudioManager>();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        am.Play("Theme");
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
