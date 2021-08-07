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
    public bool loadGame;
    public bool aiYellow;
    public bool aiRed;
    public bool debug;
    public bool mixed;
    public int rockCurrent;
    public int endCurrent;
    public int yellowScore;
    public int redScore;

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

        //if (loadGame)
        //{
        //    redHammer = data.redHammer;
        //    ends = data.endTotal;
        //    aiYellow = data.aiYellow;
        //    rockCurrent = data.rockCurrent;
        //    endCurrent = data.endCurrent;
        //}
        //else LoadSettings();

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
        if (gs)
        {
            gs = GameObject.FindGameObjectWithTag("GameSettings").GetComponent<GameSettings>();

            debug = gs.debug;
            ends = gs.ends;
            rocks = gs.rocks;
            volume = gs.volume;
            redHammer = gs.redHammer;
            aiYellow = gs.ai;
            mixed = gs.mixed;
            loadGame = gs.loadGame;
            
        }
    }

    public void AutoSave()
    {
        //GameData data = SaveSystem.LoadPlayer();
        //ends = data.endTotal;
        //endCurrent = data.endCurrent;
        //rocks = data.rocks;
        //rockCurrent = data.rockCurrent;
        //redHammer = data.redHammer;
        //aiYellow = data.aiYellow;
        //yellowScore = data.yellowScore;
        //redScore = data.redScore;
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
