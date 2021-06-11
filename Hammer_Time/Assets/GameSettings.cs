using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSettings : MonoBehaviour
{
    public bool redHammer;
    // Start is called before the first frame update

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void SetHammerRed()
    {
        redHammer = true;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void SetHammerYellow()
    {
        redHammer = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
