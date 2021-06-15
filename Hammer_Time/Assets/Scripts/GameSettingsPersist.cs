using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSettingsPersist : MonoBehaviour
{
    GameSettings gs;
    public bool redHammer;
    public int ends;
    public int rocks;
    public float volume;
    public bool tutorial;
    public static GameSettingsPersist instance;
    // Start is called before the first frame update

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

        if (tutorial)
        {
            OnTutorial();
        }
        else LoadSettings();
    }

    private void Start()
    {
    }

    public void LoadSettings()
    {
            gs = GameObject.FindGameObjectWithTag("GameSettings").GetComponent<GameSettings>();
            ends = gs.ends;
            rocks = gs.rocks;
            volume = gs.volume;
            redHammer = gs.redHammer;
    }

    private void Update()
    {
        if (tutorial)
        {
            OnTutorial();
            tutorial = false;
        }
    }

    public void OnTutorial()
    {
        ends = 10;
        rocks = 8;
        redHammer = true;
        //GameManager gm = FindObjectOfType<GameManager>();

        //gm.endCurrent = 10;
    }
}
