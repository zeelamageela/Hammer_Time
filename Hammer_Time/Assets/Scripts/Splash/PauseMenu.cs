using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject pauseMenuUI;
    public GameHUD gHUD;
    AudioManager am;

    public Slider volumeSlider;
    public float volume;
    // Update is called once per frame
    private void Start()
    {
        am = FindObjectOfType<AudioManager>();
        am.PlayBG(3);
    }
    void Update()
    {
        if (GameIsPaused)
        {
            AudioManager am = FindObjectOfType<AudioManager>();
            volume = volumeSlider.value;
            am.Volume(volume);
            
            am.maxVol = volume;

        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameIsPaused)
            {
                Resume();
                
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        Time.timeScale = 1f;
        pauseMenuUI.SetActive(false);
        GameIsPaused = false;
        AudioManager am = FindObjectOfType<AudioManager>();
        am.PlayBG(0);
    }

    public void Pause()
    {
        Time.timeScale = 0f;
        pauseMenuUI.SetActive(true);
        GameIsPaused = true;
        gHUD.scoreCheck = true;
        volumeSlider.value = am.maxVol;
        am.PlayBG(3);
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;
        AudioManager am = FindObjectOfType<AudioManager>();
        am.Stop("Ambience");
        SceneManager.LoadScene("SplashMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
