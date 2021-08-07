using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TigerForge;

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
    public EasyFileSave myFile;

    public static GameSettingsPersist instance;

    // Start is called before the first frame update

    private void Awake()
    {
        myFile = new EasyFileSave("my_game_data");
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
    }

    private void Start()
    {

        //gs = GameObject.Find("GameSettings").GetComponent<GameSettings>();

        if (tutorial)
        {
            OnTutorial();
        }
        
        if (loadGame)
        {
            LoadGame();
        }
    }

    public void LoadSettings()
    {
        gs = GameObject.Find("GameSettings").GetComponent<GameSettings>();
        //load all the saved values
        ends = gs.ends;
        endCurrent = 0;
        rocks = gs.rocks;
        rockCurrent = 0;
        redHammer = gs.redHammer;
        aiYellow = gs.ai;
        mixed = gs.mixed;
    }

    public void LoadGame()
    {

        //load all the saved values
        ends = myFile.GetInt("End Total");
        endCurrent = myFile.GetInt("Current End");
        rocks = myFile.GetInt("Rock Total");
        rockCurrent = myFile.GetInt("Current Rock");
        redHammer = myFile.GetBool("Red Hammer");
        aiYellow = myFile.GetBool("Ai Yellow");
        mixed = myFile.GetBool("Mixed");
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

        if (gs)
        {
            ends = gs.ends;
            rocks = gs.rocks;
            aiYellow = gs.ai;
        }
        else loadGame = true;
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
