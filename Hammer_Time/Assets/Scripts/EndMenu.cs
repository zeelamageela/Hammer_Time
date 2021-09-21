using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class EndMenu : MonoBehaviour
{
    GameSettingsPersist gsp;
    public Text endNumber;
    public Text redScore;
    public Text yellowScore;
    public GameObject yellowSpinner;
    public GameObject yellowSpinnerAI;
    public GameObject redHammerPNG;
    public GameObject yellowHammerPNG;
    public string contScene;

    // Start is called before the first frame update
    void Start()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();

        if (gsp)
        {
            if (gsp.endCurrent >= gsp.ends && gsp.redScore != gsp.yellowScore)
            {
                endNumber.text = "Game Over!";
            }
            else
            {
                endNumber.text = gsp.endCurrent.ToString();
                redScore.text = gsp.redScore.ToString();
                yellowScore.text = gsp.yellowScore.ToString();

                if (gsp.redHammer)
                    redHammerPNG.SetActive(true);
                else
                    yellowHammerPNG.SetActive(true);
            }

            if (gsp.aiYellow)
            {
                yellowSpinner.SetActive(false);
                yellowSpinnerAI.SetActive(true);
            }
            else
            {
                yellowSpinner.SetActive(true);
                yellowSpinnerAI.SetActive(false);
            }


        }
        
    }

    public void Menu()
    {
        SceneManager.LoadScene("SplashMenu");
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void Continue()
    {
        SceneManager.LoadScene(contScene);
    }
}
