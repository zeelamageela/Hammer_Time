using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using TigerForge;
using Lofelt.NiceVibrations;

public enum GameState { START, DRAWTOBUTTON, REDTURN, YELLOWTURN, CHECKSCORE, SCORE, RESET, END, DEBUG }


public class GameManager : MonoBehaviour
{
    #region Variables
    AudioManager am;
    //public TutorialManager tm;
    public TeamManager teamM;
    public AIManager aim;
    public StoryManager storyM;

    public int endCurrent;
    public int endTotal;
    public bool redHammer;
    public int rocksPerTeam;
    public int rockTotal;
    public int rockCurrent;

    public GameSettingsPersist gsp;
    public RockManager rm;
    public GameObject redShooter;
    public GameObject yellowShooter;
    public GameObject shooterAnimRed;
    public GameObject shooterAnimYellow;
    public GameObject shooterAnimRedRock;
    public GameObject shooterAnimYellowRock;

    public SweeperManager sm;

    public GameObject shooterGO;
    public Transform launcher;
    public Transform yellowRocksInactive;
    public Transform redRocksInactive;
    public Collider2D boardCollider;
    public Collider2D launchCollider;

    public GameObject yellowSpin;
    public GameObject redSpin;

    public string redTeamName;
    public int redScore;
    public string yellowTeamName;
    public int yellowScore;

    //public int[] endScoreRed;
    //public int[] endScoreYellow;

    public LoadGame lg;

    public GameHUD gHUD;
    public RockBar rockBar;

    public bool aiTeamRed;
    public bool aiTeamYellow;
    public bool target;
    public bool mixed;
    public bool multiplayer;
    public bool debug;
    public Text dbText;

    public GameState state;
    public bool redTurn;
    Rock_Info redRock;
    Rock_Info yellowRock;

    public DialogueManager coachGreen;

    [Header("Dialogue")]
    [Tooltip("Dialogue to show at tournament intro")]
    public Canvas dialogueCanvas;
    public DialogueData Coach_Welcome;
    public DialogueData[] Announcer_Welcome;
    public DialogueData[] Announcer_Winning_By_1;
    public DialogueData[] Announcer_Losing_By_1;
    public DialogueData[] Announcer_Winning_By_Many;
    public DialogueData[] Announcer_Losing_By_Many;
    public DialogueData[] Announcer_Winning_By_1_Late;
    public DialogueData[] Announcer_Losing_By_1_Late;
    public DialogueData[] Announcer_Winning_By_Many_Late;
    public DialogueData[] Announcer_Losing_By_Many_Late;
    public DialogueData[] Announcer_Won;
    public DialogueData[] Announcer_Lost;

    public GameObject targetButtons;
    public GameObject targetAi;
    public GameObject targetPlayer;
    public GameObject targetStory;

    public GameObject db;
    public GameObject dbrandom;

    public CameraManager cm;
    public GameObject vcam_go;
    //public CinemachineVirtualCamera vcam;
    Transform tFollowTarget;

    public ShootingKnob knob;
    [SerializeField]
    public List<Rock_List> rockList;
    [SerializeField]
    public List<House_List> houseList;
    [SerializeField]
    public List<Guard_List> gList;

    public Animator tournyIntro;
    public GameObject tournyIntroGO;

    EasyFileSave myFile;
    #endregion

    void Start()
    {
        // ? DIAGNOSTIC: Check score array at VERY START of GameManager.Start()
        GameSettingsPersist gspCheck = FindObjectOfType<GameSettingsPersist>();
        if (gspCheck != null && gspCheck.score != null && gspCheck.score.Length > 0)
        {
            Debug.Log($"[GameManager.Start] === ENTRY === score array: End1=({gspCheck.score[0].x},{gspCheck.score[0].y}), End2=({(gspCheck.score.Length > 1 ? gspCheck.score[1].x : -1)},{(gspCheck.score.Length > 1 ? gspCheck.score[1].y : -1)})");
        }
        else
        {
            Debug.Log($"[GameManager.Start] === ENTRY === score array is NULL or empty!");
        }
        
        state = GameState.START;

        GameObject boards = GameObject.Find("BG/Boards_CREATED");
        boardCollider = boards.GetComponent<Collider2D>();
        myFile = new EasyFileSave("my_game_data");
        am = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
        StartCoroutine(SetupGame());
    }

    private string InferTeamNameFromRockIndex(int rockIndex)
    {
        if (string.IsNullOrWhiteSpace(redTeamName) || string.IsNullOrWhiteSpace(yellowTeamName))
            return string.Empty;

        int notHammer = redHammer ? 1 : 0;
        int i = rockIndex + 1;
        bool isYellowRock = (i % 2) == notHammer;
        return isYellowRock ? yellowTeamName : redTeamName;
    }

    private void SyncRockTeamNamesIfMissing()
    {
        if (rockList == null)
            return;

        foreach (Rock_List rockEntry in rockList)
        {
            if (rockEntry == null || rockEntry.rockInfo == null)
                continue;

            if (string.IsNullOrWhiteSpace(rockEntry.rockInfo.teamName))
            {
                rockEntry.rockInfo.teamName = InferTeamNameFromRockIndex(rockEntry.rockInfo.rockIndex);
            }

            if (rockEntry.rock != null && !string.IsNullOrWhiteSpace(rockEntry.rockInfo.teamName))
            {
                string expectedPrefix = rockEntry.rockInfo.teamName + " ";
                if (!rockEntry.rock.name.StartsWith(expectedPrefix))
                {
                    rockEntry.rock.name = rockEntry.rockInfo.teamName + " " + rockEntry.rockInfo.rockNumber;
                }
            }
        }
    }

    #region "Setup and Reset"
    IEnumerator SetupGame()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
        // If running standalone (tutorial) scene, create a minimal runtime GSP to avoid NREs
        if (gsp == null)
        {
            Debug.Log("GameManager: No GameSettingsPersist found, creating runtime instance with tutorial defaults.");
            GameObject gspGO = new GameObject("GameSettingsPersist_Runtime");
            gsp = gspGO.AddComponent<GameSettingsPersist>();
            gsp.tutorial = true;
            gsp.ends = 8;
            gsp.rocks = 8;
            gsp.redHammer = true;
            gsp.aiYellow = true;
            gsp.aiRed = false;
            gsp.mixed = false;
            gsp.debug = false;
            gsp.loadGame = false;
            gsp.bg = 0;
            gsp.redTeamName = "Newbie";
            gsp.yellowTeamName = "Opponent";
            gsp.score = new Vector2Int[gsp.ends + 1];
            for (int i = 0; i < gsp.score.Length; i++) gsp.score[i] = new Vector2Int(0, 0);
        }
        //gHUD.SetHUD(redRock);
        Debug.Log("[GameManager] Game Start - loadGame: " + (gsp.loadGame ? "TRUE" : "FALSE"));


        Rock_Placement rp = FindFirstObjectByType<Rock_Placement>();
        
        
        // CRITICAL FIX: Don't set gameInProgress here if loading from save!
        // LoadGame() will handle the loaded game state
        if (!gsp.loadGame)
        {
            gsp.gameInProgress = true;
            Debug.Log("[GameManager] NEW game - set gameInProgress = true");
        }
        else
        {
            gsp.LoadTourny();
            Debug.Log("[GameManager] LOADING game - gameInProgress preserved from save: " + gsp.gameInProgress);
        }
        endCurrent = gsp.endCurrent;
        rockCurrent = gsp.rockCurrent;
        rocksPerTeam = gsp.rocks;
        Debug.Log($"[GameManager] After load/setup: rocksPerTeam={rocksPerTeam}, gsp.rocks={gsp.rocks}, rockCurrent={rockCurrent}");
        redHammer = gsp.redHammer;
        endTotal = gsp.ends;
        //rockTotal = 16;
        aiTeamYellow = gsp.aiYellow;
        aiTeamRed = gsp.aiRed;
        mixed = gsp.mixed;

        // CRITICAL FIX: Only set default rockCurrent for NEW games
        // For loaded games, preserve the rockCurrent from the save (set above at line 150)
        if (!gsp.loadGame)
        {
            // rockCurrent = first player rock index (number of rocks to sim before player starts)
            // If gsp.rocks=8: firstPlayerRock=0, no sims needed
            // If gsp.rocks=1: firstPlayerRock=14, sim rocks 0-13 first
            int firstPlayerRock = 16 - (gsp.rocks * 2);
            rockCurrent = firstPlayerRock;
            gsp.rockCurrent = rockCurrent;
            Debug.Log($"[GameManager] NEW game - rockCurrent={rockCurrent} (firstPlayerRock={firstPlayerRock})");
        }
        else
        {
            Debug.Log($"[GameManager] LOADED game - rockCurrent preserved from save: {rockCurrent}");
        }
        
        // CRITICAL FIX: Initialize score array at start of NEW game only!
        // Don't clear it when continuing to next end (endCurrent > 0)
        if (gsp.score == null || gsp.score.Length < (endTotal + 1))
        {
            Debug.Log($"[GameManager] Score array needs resize - current: {(gsp.score == null ? "NULL" : gsp.score.Length.ToString())}, needed: {endTotal + 1}");
            
            // CRITICAL: PRESERVE existing scores when resizing!
            Vector2Int[] newArray = new Vector2Int[endTotal + 1];
            if (gsp.score != null)
            {
                // Copy all existing scores
                for (int i = 0; i < gsp.score.Length; i++)
                {
                    newArray[i] = gsp.score[i];
                }
                Debug.Log($"[GameManager] Copied {gsp.score.Length} existing scores to new array");
            }
            gsp.score = newArray;
        }
        else if (gsp.endCurrent == 0)
        {
            // Only clear when starting a NEW game (endCurrent == 0)
            // Don't clear when continuing to next end!
            Debug.Log($"[GameManager] Clearing score array for NEW game (endCurrent=0)");
            for (int i = 0; i < gsp.score.Length; i++)
            {
                gsp.score[i] = new Vector2Int(0, 0);
            }
        }
        else
        {
            // Preserve existing scores
            Debug.Log($"[GameManager] Preserving score array for end {gsp.endCurrent + 1}");
            Debug.Log($"[GameManager]   Current scores: End1=({gsp.score[0].x},{gsp.score[0].y}), End2=({(gsp.score.Length > 1 ? gsp.score[1].x : -1)},{(gsp.score.Length > 1 ? gsp.score[1].y : -1)})");
        }

        if (gsp.redScore > 0 | gsp.yellowScore > 0)
        {
            redScore = gsp.redScore;
            yellowScore = gsp.yellowScore;
            cm.ui.enabled = false;
        }

        redTeamName = gsp.redTeamName;
        yellowTeamName = gsp.yellowTeamName;

        debug = gsp.debug;

        Debug.Log("redHammer is " + redHammer);
        am.PlayBG(0);
        am.Play("Ambience");
        targetAi.SetActive(false);
        targetPlayer.SetActive(false);
        targetStory.SetActive(false);

        rockList = new List<Rock_List>();
        houseList = new List<House_List>();
        gList = new List<Guard_List>();
        //endScoreRed = new int[endTotal];
        //endScoreYellow = new int[endTotal];
        //yield return new WaitForSeconds(2f);
        yield return new WaitForEndOfFrame();

        gHUD.SetHammer(redHammer);

        if (cm != null && cm.ui != null)
            cm.ui.enabled = true;

        if (endCurrent > 1)
        {
            for (int i = 0; i < endCurrent; i++)
            {
                //gHUD.ScoringPanel();
            }
        }
        // else
        // {
        //     // First end, show intro dialogue
        //     yield return StartCoroutine(TournyIntro());
        // }
        if (gsp.loadGame)
        {
            StartCoroutine(LoadGame());
        }
        //else if (gsp.story)
        //{
        //    StartCoroutine(StoryLoad());
        //}
        else
        {
            yield return StartCoroutine(SetupRocks());

            yield return new WaitUntil(() => rockList.Count == 16);

            // TUTORIAL HOOK: Replace simulation loop with pre-configured rock placement
            TutorialGameManager tutorialGM = TutorialGameManager.Instance;
            if (tutorialGM != null && tutorialGM.isTutorialGame)
            {
                yield return StartCoroutine(tutorialGM.RepositionPreConfiguredRocks());
                rockCurrent--;  // Step back so CheckScore → NextTurn lands on first player rock
                rockBar.ResetBar(redHammer);
                rockBar.EndUpdate(yellowScore, redScore);
                yield return new WaitUntil(() => rockBar.rockListUI.Count == 16);
                rm.rrp.placed = true;
                StartCoroutine(CheckScore());
                yield break;
            }

            // Show intro for tournament games on the first played game in the tournament
            if (!gsp.tournyIntroShown && !gsp.cashGame && !gsp.debug)
            {
                yield return StartCoroutine(TournyIntro());
                gsp.tournyIntroShown = true;  // Mark intro as shown
                Debug.Log("[GameManager] Tourny intro shown and flag set");
            }

            if (rockCurrent > 0)
            {
                cm.HouseView();
                rockBar.ResetBar(redHammer);
                int tempRckCrnt = rockCurrent;
                
                // Place all rocks instantly without delays
                for (int i = 0; i < rockCurrent; i++)
                {
                    rm.rrp.placed1 = false;
                    if (gsp.cashGame)
                    {
                        if (gsp.aiRed)
                        {
                            rm.rrp.OnRockPlace(i, false, true);
                        }
                        else if (gsp.aiYellow)
                        {
                            rm.rrp.OnRockPlace(i, true, true);
                        }

                        rm.rrp.placed1 = true;
                    }
                    else
                    {
                        if (gsp.aiRed)
                        {
                            yield return rm.rrp.OnRockPlace(i, false);
                        }
                        else if (gsp.aiYellow)
                        {
                            yield return rm.rrp.OnRockPlace(i, true);
                        }
                    }

                    if (i % 2 == 0)
                    {
                        if (redHammer)
                            rockBar.ActiveRock(false, i);
                        else
                            rockBar.ActiveRock(true, i);
                    }
                    else
                    {
                        if (redHammer)
                            rockBar.ActiveRock(true, i);
                        else
                            rockBar.ActiveRock(false, i);
                    }

                    rockBar.ShotUpdate(i, rockList[i].rockInfo.outOfPlay);
                    
                    // Wait for rock bar UI to be ready (only once)
                    if (i == 0)
                        yield return new WaitUntil(() => rockBar.rockListUI.Count == 16);

                    yield return new WaitUntil(() => rm.rrp.placed1);

                    gHUD.MainDisplayOff();
                }
                rockCurrent--;

                rockBar.EndUpdate(yellowScore, redScore);
                StartCoroutine(CheckScore());
            }
            else
            {
                rockBar.ResetBar(redHammer);
                rockBar.EndUpdate(yellowScore, redScore);
                yield return new WaitUntil(() => rockBar.rockListUI.Count == 16);
                rm.rrp.placed = true;
                if (redHammer)
                    OnYellowTurn();
                else
                    OnRedTurn();
            }
        }
    }


    IEnumerator SetupRocks()
    {
        int hammer;
        int notHammer;

        if (redHammer)
        {
            hammer = 0;
            notHammer = 1;
        }
        else
        {
            hammer = 1;
            notHammer = 0;
        }

        for (int i = 1; i <= 16; i++)
        {
            HapticPatterns.PlayPreset(HapticPatterns.PresetType.SoftImpact);
            if (i % 2 == notHammer)
            {
                GameObject yellowRock_go;

                yellowRock_go = Instantiate(yellowShooter, yellowRocksInactive);

                int yRocks = 4;
                int k = (i / 2) + notHammer;

                if (k <= yRocks)
                {
                    yellowRock_go.transform.position = new Vector2(yellowRock_go.transform.position.x, yellowRock_go.transform.position.y - ((k - 1) * 0.4f));
                }
                else if (k > yRocks)
                {
                    float j = k - yRocks;
                    yellowRock_go.transform.position = new Vector2(yellowRock_go.transform.position.x - 0.4f, yellowRock_go.transform.position.y - ((j - 1) * 0.4f));
                }

                Rock_Info yellowRock_info = yellowRock_go.GetComponent<Rock_Info>();
                yellowRock_info.rockNumber = k;
                yellowRock_info.rockIndex = i - 1;
                yellowRock_info.teamName = yellowTeamName;
                yellowRock_go.name = yellowTeamName + " " + yellowRock_info.rockNumber;
                yellowRock_go.GetComponent<Rock_Flick>().enabled = false;
                yellowRock_go.GetComponent<Rock_Release>().enabled = false;
                yellowRock_go.GetComponent<Rock_Force>().enabled = false;
                yellowRock_go.GetComponent<CircleCollider2D>().enabled = false;
                rockList.Add(new Rock_List(yellowRock_go, yellowRock_info));
                if (gsp.debug)
                    yield return new WaitForSeconds(0.025f);
                else
                    yield return null;
            }
            if (i % 2 == hammer)
            {
                GameObject redRock_go;

                redRock_go = Instantiate(redShooter, redRocksInactive);

                int yRocks = 4;
                int k = (i / 2) + hammer;
                if (k <= yRocks)
                {
                    redRock_go.transform.position = new Vector2(redRock_go.transform.position.x, redRock_go.transform.position.y - ((k - 1) * 0.4f));
                }
                else if (k > yRocks)
                {
                    float j = k - yRocks;
                    redRock_go.transform.position = new Vector2(redRock_go.transform.position.x + 0.4f, redRock_go.transform.position.y - ((j - 1) * 0.4f));
                }


                Rock_Info redRock_info = redRock_go.GetComponent<Rock_Info>();
                redRock_info.rockNumber = k;
                redRock_info.rockIndex = i - 1;
                redRock_info.teamName = redTeamName;
                redRock_go.name = redTeamName + " " + redRock_info.rockNumber;

                redRock_go.GetComponent<CircleCollider2D>().enabled = false;
                redRock_go.GetComponent<Rock_Flick>().enabled = false;
                redRock_go.GetComponent<Rock_Release>().enabled = false;
                redRock_go.GetComponent<Rock_Force>().enabled = false;
                rockList.Add(new Rock_List(redRock_go, redRock_info));
                if (gsp.debug)
                    yield return new WaitForSeconds(0.025f);
                else
                    yield return null;
            }
            //rockList.Sort();
        }
    }
    IEnumerator ResetGame()
    {
        gList.Clear();
        houseList.Clear();

        if (rockList.Count != 0)
        {
            foreach (Rock_List rock in rockList)
            {
                Destroy(rock.rock);
            }
        }

        yield return new WaitForFixedUpdate();

        rockList.Clear();

        endCurrent++;
        // rockCurrent = first player rock index (number of rocks to sim before player starts)
        int firstPlayerRock = 16 - (rocksPerTeam * 2);
        rockCurrent = firstPlayerRock;
        gsp.rockCurrent = rockCurrent;
        Debug.Log($"[GM.CheckScore] New end {endCurrent} - rockCurrent={rockCurrent} (firstPlayerRock={firstPlayerRock})");
        gHUD.SetHammer(redHammer);

        yield return StartCoroutine(SetupRocks());
        yield return new WaitUntil(() => rockList.Count == 16);

        // Show announcer dialogue at the START of the end (before placing rocks)
        // Skip on end 1 (endCurrent == 0) as that's the very first end with no score yet
        if (endCurrent >= 1 && !gsp.cashGame && !gsp.debug)
        {
            DialogueData selectedDialogue = SelectDialogueBasedOnGameState();
            if (selectedDialogue != null && DialogueController.Instance != null)
            {
                // Enable dialogue canvas before showing
                if (dialogueCanvas != null)
                    dialogueCanvas.enabled = true;
                    
                DialogueController.Instance.Show(selectedDialogue);
                // Wait for dialogue to complete before continuing
                yield return new WaitUntil(() => !DialogueController.Instance.IsShowing);
                Debug.Log("[GameManager] Announcer dialogue shown at start of end " + endCurrent);
            }
        }

        if (rockCurrent > 0)
        {
            if (gsp.aiRed)
            {
                rm.rrp.OnRockPlace(rockCurrent, false);
            }
            else
            {
                rm.rrp.OnRockPlace(rockCurrent, true);
            }
            yield return new WaitUntil(() => rm.rrp.placed);
            rockBar.ResetBar(redHammer);
            yield return new WaitUntil(() => rockBar.rockListUI.Count == 16);
            //if (redHammer)
            //    OnYellowTurn();
            //else
            //    OnRedTurn();
            //StartCoroutine(SaveGame());
            yield return new WaitUntil(() => rm.rrp.placed);
            StartCoroutine(CheckScore());
        }
        else
        {
            // No sims needed - rockCurrent is 0, start first rock directly
            rockBar.ResetBar(redHammer);
            rockBar.EndUpdate(yellowScore, redScore);
            yield return new WaitUntil(() => rockBar.rockListUI.Count == 16);
            rm.rrp.placed = true;
            if (redHammer)
                OnYellowTurn();
            else
                OnRedTurn();
        }
        
    }
    #endregion

    #region Turns
    public void OnRedTurn()
    {
        // PLAYER TURN: Initialize to default out-turn, player can toggle via button
        if (!aiTeamRed)
        {
            rm.inturn = false;  // Default to out-turn for player turns
            Debug.Log($"[GameManager] Player Red Turn - initialized rm.inturn=false (OUT-TURN default)");
        }
        
        // Clear trajectory from previous turn
        TrajectoryLine trajectoryLine = FindAnyObjectByType<TrajectoryLine>();
        if (trajectoryLine != null)
        {
            trajectoryLine.ClearTrajectory();
            
            // Only enable line renderer if this is a player turn (not AI)
            if (!aiTeamRed)
            {
                trajectoryLine.ShowTrajectoryLine();
            }
        }
        
        cm.ShotSetup();

        state = GameState.REDTURN;
        //Debug.Log("Red Turn");
        redTurn = true;
        am.PlayBG(2);

        if (GameObject.FindGameObjectsWithTag("Player").Length >= 1)
        {
            Destroy(GameObject.FindGameObjectWithTag("Player"));
        }

        teamM.SetCharacter(rockCurrent, true);

        shooterGO = Instantiate(shooterAnimRed);
        Instantiate(shooterAnimRedRock, shooterGO.transform);
        sm.SetupSweepers(true);


        GameObject redRock_1 = rockList[rockCurrent].rock;
        redRock_1.GetComponent<Rock_Flick>().enabled = true;
        redRock_1.GetComponent<Rock_Release>().enabled = true;
        redRock_1.GetComponent<Rock_Force>().enabled = true;
        redRock_1.GetComponent<Rock_Colliders>().enabled = true;
        
        // CRITICAL: For player turns, initialize flipAxis from rm.inturn immediately after enabling Rock_Force
        // This ensures the rock starts with the correct default turn before the player can toggle it
        // BOTH values must be synchronized from the start!
        if (!aiTeamRed)
        {
            Rock_Force redRockForce = redRock_1.GetComponent<Rock_Force>();
            if (redRockForce != null)
            {
                redRockForce.flipAxis = rm.inturn;
                Debug.Log($"[GameManager.OnRedTurn] ? SYNCED: rm.inturn={rm.inturn}, rock.flipAxis={rm.inturn}");
            }
        }
        
        boardCollider.enabled = false;
        launchCollider.enabled = false;
        
        //StartCoroutine(SaveGame());
        StartCoroutine(RedTurn());
    }

    IEnumerator RedTurn()
    {
        GameObject redRock_1 = rockList[rockCurrent].rock;

        redRock = redRock_1.GetComponent<Rock_Info>();
        Debug.Log(redRock_1.name);

        //Debug.Log("Current Rock is " + rockCurrent);
        rockBar.ActiveRock(true, rockCurrent);

        if (debug)
        {
            dbText.enabled = true;
        }

        if (aiTeamRed)
        {
            gHUD.Message(redTeamName + " Turn");

            yield return new WaitForSeconds(1f);

            if (rockCurrent >= 14)
                gHUD.Message(redTeamName + "'s Last Rock");
            if (rockCurrent >= 15)
                gHUD.Message("Last Rock");
            aim.OnShot(rockCurrent);
        }
        else if (target)
        {
            cm.HouseView();
            targetPlayer.SetActive(true);
            targetButtons.SetActive(true);
            if (debug)
            {
                dbText.enabled = true;
            }
        }
        else
        {
            yield return new WaitForSeconds(1f);

            if (rockCurrent >= 14)
                gHUD.Message(redTeamName + "'s Last Rock");
            if (rockCurrent >= 15)
                gHUD.Message("Last Rock");

            // TUTORIAL HOOK: Fire tutorial sequence for this player rock
            TutorialGameManager tutGM = TutorialGameManager.Instance;
            if (tutGM != null && tutGM.isTutorialGame && tutGM.ShouldTriggerTutorial(rockCurrent, out string redTutorialId))
            {
                if (TutorialSequenceManager.Instance != null)
                    TutorialSequenceManager.Instance.PlaySequence(redTutorialId);
            }
        }

        yield return new WaitUntil(() => redRock.shotTaken == true);

        if (target)
        {
            targetButtons.SetActive(false);
            targetPlayer.SetActive(false);
        }
        
        //am.Play("RockScrape");
        cm.RockFollow(redRock_1.transform);
        boardCollider.enabled = true;

        rockBar.ActiveRock(true, rockCurrent);

        yield return new WaitUntil(() => redRock.released == true);

        gHUD.mainDisplay.enabled = false;

        if (aiTeamRed)
        {
            targetAi.SetActive(false);
        }
        redRock_1.GetComponent<Rock_Flick>().enabled = false;
        sm.Release(redRock_1, aiTeamRed);
        rm.GetComponent<Sweep>().EnterSweepZone();

        yield return new WaitUntil(() => redRock.rest == true);
        if (debug)
        {
            dbText.enabled = false;
        }
        //am.Stop("RockScrape");
        rm.GetComponent<Sweep>().ExitSweepZone();

        Debug.Log("redRock at Rest");
        
        // Hide trajectory line at end of turn (will be redrawn when next player turn starts)
        TrajectoryLine trajectoryLine = FindAnyObjectByType<TrajectoryLine>();
        if (trajectoryLine != null)
        {
            trajectoryLine.HideTrajectoryLine();
        }
        //vcam.enabled = false;

        StartCoroutine(AllStopped());

        if (!redRock.outOfPlay)
        {
            if (redRock.inPlay && !redRock.inHouse)
            {
                if (redRock_1.transform.position.y <= 6.5f)
                {
                    redRock.freeGuard = true;

                    gList.Add(new Guard_List(rockCurrent, redRock.freeGuard, redRock_1.transform));
                }
            }
            rockBar.IdleRock(rockCurrent);
        }
        else
        {
            rockBar.DeadRock(rockCurrent);
        }

        StartCoroutine(CheckScore());
    }

    public void OnYellowTurn()
    {
        // PLAYER TURN: Initialize to default out-turn, player can toggle via button
        if (!aiTeamYellow)
        {
            rm.inturn = false;  // Default to out-turn for player turns
            Debug.Log($"[GameManager] Player Yellow Turn - initialized rm.inturn=false (OUT-TURN default)");
        }
        
        // Clear trajectory from previous turn
        TrajectoryLine trajectoryLine = FindAnyObjectByType<TrajectoryLine>();
        if (trajectoryLine != null)
        {
            trajectoryLine.ClearTrajectory();
            
            // Only enable line renderer if this is a player turn (not AI)
            if (!aiTeamYellow)
            {
                trajectoryLine.ShowTrajectoryLine();
            }
        }
        
        state = GameState.YELLOWTURN;
        Debug.Log("Yellow Turn");

        redTurn = false;

        am.PlayBG(0);

        cm.ShotSetup();

        if (GameObject.FindGameObjectsWithTag("Player").Length >= 1)
        {
            Destroy(GameObject.FindGameObjectWithTag("Player"));
        }

        teamM.SetCharacter(rockCurrent, false);
        shooterGO = Instantiate(shooterAnimYellow);

        Instantiate(shooterAnimYellowRock, shooterGO.transform);

        GameObject yellowRock_1 = rockList[rockCurrent].rock;
        yellowRock_1.GetComponent<Rock_Flick>().enabled = true;
        yellowRock_1.GetComponent<Rock_Release>().enabled = true;
        yellowRock_1.GetComponent<Rock_Force>().enabled = true;
        yellowRock_1.GetComponent<Rock_Colliders>().enabled = true;
        
        // CRITICAL: For player turns, initialize flipAxis from rm.inturn immediately after enabling Rock_Force
        // This ensures the rock starts with the correct default turn before the player can toggle it
        // BOTH values must be synchronized from the start!
        if (!aiTeamYellow)
        {
            Rock_Force yellowRockForce = yellowRock_1.GetComponent<Rock_Force>();
            if (yellowRockForce != null)
            {
                yellowRockForce.flipAxis = rm.inturn;
                Debug.Log($"[GameManager.OnYellowTurn] ? SYNCED: rm.inturn={rm.inturn}, rock.flipAxis={rm.inturn}");
            }
        }
        
        boardCollider.enabled = false;
        launchCollider.enabled = false;

        sm.SetupSweepers(false);
        StartCoroutine(YellowTurn());
    }

    IEnumerator YellowTurn()
    {
        GameObject yellowRock_1 = rockList[rockCurrent].rock;
        //Debug.Log("rmInturn is " + rm.inturn);

        yellowRock = yellowRock_1.GetComponent<Rock_Info>();
        Debug.Log(yellowRock_1.name);

        rockBar.ActiveRock(false, rockCurrent);
        if (debug)
        {
            dbText.enabled = true;
        }

        if (aiTeamYellow)
        {
            gHUD.Message(yellowTeamName + " Turn");

            if (debug)
            {
                dbText.enabled = true;
            }

            yield return new WaitForSeconds(1f);

            if (rockCurrent >= 14)
                gHUD.Message(yellowTeamName + "'s Last Rock");
            if (rockCurrent >= 15)
                gHUD.Message(yellowTeamName + " Last Rock");

            aim.OnShot(rockCurrent);
        }
        else if (target)
        {
            cm.HouseView();
            targetPlayer.SetActive(true);
            targetButtons.SetActive(true);
        }
        else
        {
            yield return new WaitForSeconds(1f);

            if (rockCurrent >= 14)
                gHUD.Message(yellowTeamName + "'s Last Rock");
            if (rockCurrent >= 15)
                gHUD.Message("Last Rock");

            // TUTORIAL HOOK: Fire tutorial sequence for this player rock
            TutorialGameManager tutGM = TutorialGameManager.Instance;
            if (tutGM != null && tutGM.isTutorialGame && tutGM.ShouldTriggerTutorial(rockCurrent, out string yellowTutorialId))
            {
                if (TutorialSequenceManager.Instance != null)
                    TutorialSequenceManager.Instance.PlaySequence(yellowTutorialId);
            }
        }
        yield return new WaitUntil(() => yellowRock.shotTaken == true);

        if (target)
        {
            targetButtons.SetActive(false);
            targetPlayer.SetActive(false);
        }
        
        cm.RockFollow(yellowRock_1.transform);
        //am.Play("RockScrape");
        boardCollider.enabled = true;
        rockBar.ActiveRock(false, rockCurrent);

        yield return new WaitUntil(() => yellowRock.released == true);

        sm.Release(yellowRock_1, aiTeamYellow);

        gHUD.mainDisplay.enabled = false;

        if (aiTeamYellow)
        {
            targetAi.SetActive(false);
        }

        yield return new WaitUntil(() => yellowRock.rest == true);

        if (debug)
        {
            dbText.enabled = false;
        }
        //am.Stop("RockScrape");

        if (debug)
        {
            dbText.enabled = false;
        }

        rm.GetComponent<Sweep>().ExitSweepZone();

        // Hide trajectory line at end of turn (will be redrawn when next player turn starts)
        TrajectoryLine trajectoryLine = FindAnyObjectByType<TrajectoryLine>();
        if (trajectoryLine != null)
        {
            trajectoryLine.HideTrajectoryLine();
        }

        //vcam.enabled = false;

        yield return new WaitForEndOfFrame();

        if (!yellowRock.outOfPlay)
        {
            if (yellowRock.inPlay & !yellowRock.inHouse)
            {
                if (yellowRock_1.transform.position.y <= 6.5f)
                {
                    if (rockCurrent < 6)
                    {
                        yellowRock.freeGuard = true;
                    }

                    gList.Add(new Guard_List(rockCurrent, yellowRock.freeGuard, yellowRock_1.transform));

                }
            }

            Debug.Log("Out Of Play is " + yellowRock.outOfPlay);
            Debug.Log("Rock Current is " + rockCurrent);
            rockBar.IdleRock(rockCurrent);
        }
        else
        {
            Debug.Log("Out Of Play is " + yellowRock.outOfPlay);
            Debug.Log("Rock Current is " + rockCurrent);
            rockBar.DeadRock(rockCurrent);
        }

        StartCoroutine(CheckScore());

    }
    #endregion

    #region End of Turn
    IEnumerator AllStopped()
    {
        foreach (Rock_List rock in rockList)
        {
            Rigidbody2D rockRB = rock.rock.GetComponent<Rigidbody2D>();
            if (Mathf.Abs(rockRB.linearVelocity.y) > 0.01f && Mathf.Abs(rockRB.linearVelocity.x) > 0.01f)
            {
                //Debug.Log(rock.rockInfo.teamName + " " + rock.rockInfo.rockNumber + " is still moving :(");
                yield return new WaitUntil(() => Mathf.Abs(rockRB.linearVelocity.y) < 0.01f && Mathf.Abs(rockRB.linearVelocity.x) < 0.01f);
            }
        }
        yield return new WaitForEndOfFrame();
    }

    public IEnumerator CheckScore()
    {
        state = GameState.CHECKSCORE;


        //Debug.Log("Check Score");

        yield return StartCoroutine(AllStopped());

        Debug.Log("All Stopped");
        yield return new WaitForFixedUpdate();

        // CRITICAL FIX: Only update shot if a rock has been played (rockCurrent >= 0)
        // When starting a fresh end, rockCurrent = -1 and there's no rock to update
        if (rockCurrent >= 0 && rockCurrent < rockList.Count)
        {
            rockBar.ShotUpdate(rockCurrent, rockList[rockCurrent].rockInfo.outOfPlay);
        }
        else
        {
            Debug.Log($"[CheckScore] Skipping ShotUpdate - rockCurrent={rockCurrent} (no rocks placed yet)");
        }

        houseList.Clear();
        gList.Clear();

        Destroy(shooterGO);
        cm.HouseView();

        //Debug.Log("Rock List is " + rockList.Count);
        foreach (Rock_List rock in rockList)
        {
            if (rock.rockInfo.inHouse == true && rock.rockInfo.inPlay)
            {
                houseList.Add(new House_List(rock.rock, rock.rockInfo));
                //Debug.Log("Adding House " + rock.rockInfo.teamName + rock.rockInfo.rockNumber + " - " + rock.rockInfo.rockIndex);
            }
            if (rock.rockInfo.inPlay && !rock.rockInfo.inHouse && rock.rock.transform.position.y <= 6.5f)
            {
                gList.Add(new Guard_List(rockCurrent, rock.rockInfo.freeGuard, rock.rock.transform));
                //Debug.Log("Guard " + rock.rockInfo.name + " - " + rock.rockInfo.distance);
            }
        }

        //yield return new WaitForFixedUpdate();

        houseList.Sort();
        // if the we have shot all the rocks, go to the final scoring
        if (rockList.Count == rockCurrent + 1)
        {
            //Debug.Log("Rock List " + rockList.Count + " equals " + "Current Rock " + (rockCurrent + 1));
            StartCoroutine(Scoring());
        }
        // or else we will just check the score
        else
        {
            if (houseList.Count != 0)
            {
                int counter = 0;
                foreach (House_List rock in houseList)
                {
                    counter++;
                    Debug.Log("House List " + counter + " - " + rock.rockInfo.name + " - " + rock.rockInfo.distance);
                }
            }

            yield return new WaitForFixedUpdate();

            int houseRock = houseList.Count;

            // if the list isnt empty
            if (houseList.Count != 0)
            {
                int houseScore = 0;
                string winningTeamName = houseList[0].rockInfo.teamName;
                bool stopCounting = false;

                // lets loop the list
                for (int i = 0; i < houseList.Count; i++)
                {
                    if (!stopCounting)
                    {
                        // lets only count until the team changes
                        if (houseList[i].rockInfo.teamName == winningTeamName)
                        {
                            houseScore++;
                        }
                        else if(houseList[i].rockInfo.teamName != winningTeamName)
                        {
                            stopCounting = true;
                        }
                    }
                }
                bool noRocks = false;

                //send the info to the ui
                gHUD.CheckScore(noRocks, winningTeamName, houseScore, false);

            }
            // if the house is empty move along to next turn
            else if (houseList.Count == 0)
            {
                bool noRocks = true;
                gHUD.CheckScore(noRocks, " ", 0, false);
            }

            yield return StartCoroutine(WaitForClick());
            gHUD.clickDisplay.enabled = false;
            gHUD.MainDisplayOff();
            gHUD.ScoreboardOff();

            //Debug.Log("Current Rock is " + rockCurrent);
            NextTurn();
        }
    }

    public void NextTurn()
    {
        // ? DIAGNOSTIC: Check score array integrity at start of NextTurn
        if (gsp.score != null && gsp.score.Length > 0)
        {
            Debug.Log($"[GM.NextTurn] ENTRY - score array: End1=({gsp.score[0].x},{gsp.score[0].y}), End2=({(gsp.score.Length > 1 ? gsp.score[1].x : -1)},{(gsp.score.Length > 1 ? gsp.score[1].y : -1)})");
        }
        else
        {
            Debug.LogWarning($"[GM.NextTurn] ENTRY - score array is NULL or empty!");
        }
        
        Debug.Log("Next Turn");

        // CRITICAL FIX: Save rock positions BEFORE incrementing rockCurrent
        // This ensures we save the CURRENT state (rocks 0 to rockCurrent that have been played)
        // THEN increment for the next turn
        
        if (rm.rrp.placed)
        {
            // Save current game state BEFORE incrementing rockCurrent
            gsp.loadGame = true;
            gsp.rockPos = new Vector2[rockList.Count];
            gsp.rockInPlay = new bool[rockList.Count];
            
            for(int i = 0; i < rockList.Count; i++)
            {
                gsp.rockPos[i] = new Vector2(rockList[i].rock.transform.position.x, rockList[i].rock.transform.position.y);
                gsp.rockInPlay[i] = rockList[i].rockInfo.inPlay;
            }
            
            // CRITICAL: Save gsp.rockCurrent as the LAST rock that was played (0-indexed)
            // Don't increment it yet - that happens AFTER save
            gsp.rockCurrent = rockCurrent;
            
            Debug.Log($"[GM.NextTurn] Saving game state: rockCurrent={rockCurrent}, rocksPlayed={rockCurrent + 1}");
            
            SaveGame();
        }
        
        // NOW increment rockCurrent for the next turn
        rockCurrent++;
        
        //Debug.Log("Current Rock is " + rockCurrent);
        
        //gsp.AutoSave();
        
        // Determine if next rock is player-controlled or AI-controlled based on gsp.rocks
        // gsp.rocks = number of rocks EACH TEAM shoots (so total player rocks = gsp.rocks * 2)
        // Simmed rocks = 16 - (gsp.rocks * 2), then player shoots the final (gsp.rocks * 2) rocks
        // Example: gsp.rocks=1 → sim 0-13, player shoots 14-15
        // Example: gsp.rocks=8 → sim none, player shoots 0-15
        bool nextRockIsPlayerControlled = rockCurrent >= (16 - gsp.rocks * 2);
        
        if (nextRockIsPlayerControlled)
        {
            // Player shoots this rock manually - go to OnRedTurn/OnYellowTurn
            if (rockCurrent % 2 == 1)
            {
                if (redHammer)
                {
                    OnRedTurn();
                }
                else
                {
                    OnYellowTurn();
                }
            }
            else if (rockCurrent % 2 == 0)
            {
                if (redHammer)
                {
                    OnYellowTurn();
                }
                else
                {
                    OnRedTurn();
                }
            }
        }
        else
        {
            // AI sims this rock - call OnRockPlace for auto-placement
            if (gsp.aiRed)
            {
                rm.rrp.OnRockPlace(rockCurrent, false);
            }
            else
            {
                rm.rrp.OnRockPlace(rockCurrent, true);
            }
        }

    }
    #endregion

    #region Scoring and End of Game
    IEnumerator Scoring()
    {
        Debug.Log("Current rock is " + rockCurrent);

        state = GameState.SCORE;
        Debug.Log("Scoring");

        if (redHammer)
        {
            //Debug.Log("red hammer");
            redRock = rockList[1].rockInfo;
            yellowRock = rockList[0].rockInfo;
        }
        else
        {
            //Debug.Log("not red hammer");
            redRock = rockList[0].rockInfo;
            yellowRock = rockList[1].rockInfo;
        }

        if (houseList.Count != 0)
        {
            Debug.Log("Rocks in house");
            houseList.Clear();

            foreach (Rock_List rock in rockList)
            {
                if (rock.rockInfo.inHouse && rock.rockInfo.inPlay)
                {
                    houseList.Add(new House_List(rock.rock, rock.rockInfo));
                }
            }

            yield return new WaitForFixedUpdate();

            houseList.Sort();

            yield return new WaitForFixedUpdate();
            int houseRock = houseList.Count;

            int houseScore = 0;
            string winningTeamName = houseList[0].rockInfo.teamName;

            if (string.IsNullOrWhiteSpace(winningTeamName))
            {
                winningTeamName = InferTeamNameFromRockIndex(houseList[0].rockInfo.rockIndex);
                houseList[0].rockInfo.teamName = winningTeamName;
                Debug.LogWarning($"[GameManager.Scoring] winningTeamName was empty. Inferred '{winningTeamName}' from rock index {houseList[0].rockInfo.rockIndex}.");
            }

            bool stopCounting = false;

            // lets loop the list
            for (int i = 0; i < houseList.Count; i++)
            {
                if (string.IsNullOrWhiteSpace(houseList[i].rockInfo.teamName))
                {
                    houseList[i].rockInfo.teamName = InferTeamNameFromRockIndex(houseList[i].rockInfo.rockIndex);
                }

                if (!stopCounting)
                {
                    // lets only count until the team changes
                    if (houseList[i].rockInfo.teamName == winningTeamName)
                    {
                        houseScore++;
                    }
                    else if (houseList[i].rockInfo.teamName != winningTeamName)
                    {
                        stopCounting = true;
                    }
                }
            }

            if (redHammer)
            {
                gHUD.ScoringUI(redTeamName, winningTeamName, houseScore);
            }
            else
            {
                gHUD.ScoringUI(yellowTeamName, winningTeamName, houseScore);
            }
            Debug.Log("End Current is " + gsp.endCurrent);

            if (gsp.score.Length < 1)
                gsp.score = new Vector2Int[endTotal];
            if (endCurrent >= gsp.score.Length)
            {
                Vector2Int[] tempScore = new Vector2Int[endCurrent + 1];
                for (int i = 0; i < tempScore.Length; i++)
                {
                    if (i < gsp.score.Length)
                        tempScore[i] = gsp.score[i];
                }
                Debug.Log("Temp Score Length is " + tempScore.Length);
                gsp.score = new Vector2Int[tempScore.Length];
                gsp.score = tempScore;
            }
            if (gsp.skinsGame)
                houseScore = (int)gsp.skinValue[endCurrent];

            // ? CRITICAL FIX: Ensure score array is big enough BEFORE saving!
            if (gsp.score == null || endCurrent >= gsp.score.Length)
            {
                Debug.LogWarning($"[GameManager.Scoring] Score array too small ({gsp.score?.Length}) for endCurrent {endCurrent} - resizing NOW");
                int newSize = Mathf.Max(endTotal, endCurrent + 1);
                Vector2Int[] newScore = new Vector2Int[newSize];
                
                if (gsp.score != null)
                {
                    for (int i = 0; i < gsp.score.Length; i++)
                    {
                        newScore[i] = gsp.score[i];
                    }
                }
                
                gsp.score = newScore;
            }

            // ? CRITICAL: Save score with extra validation
            if (winningTeamName == redTeamName)
            {
                Debug.Log($"[GameManager.Scoring] Attempting to save Red score: endCurrent={endCurrent}, houseScore={houseScore}, array length={gsp.score.Length}");
                gsp.score[endCurrent] = new Vector2Int(houseScore, 0);
                Debug.Log($"[GameManager.Scoring] ? SAVED to gsp.score[{endCurrent}] = ({houseScore}, 0) for {redTeamName}");
                Debug.Log($"[GameManager.Scoring] Verification: gsp.score[{endCurrent}] now equals ({gsp.score[endCurrent].x}, {gsp.score[endCurrent].y})");
                redScore += houseScore;
                //gHUD.ScoringPanel();
                redHammer = false;
                gHUD.SetHammer(redHammer);
            }
            else if (winningTeamName == yellowTeamName)
            {
                Debug.Log($"[GameManager.Scoring] Attempting to save Yellow score: endCurrent={endCurrent}, houseScore={houseScore}, array length={gsp.score.Length}");
                gsp.score[endCurrent] = new Vector2Int(0, houseScore);
                Debug.Log($"[GameManager.Scoring] ? SAVED to gsp.score[{endCurrent}] = (0, {houseScore}) for {yellowTeamName}");
                Debug.Log($"[GameManager.Scoring] Verification: gsp.score[{endCurrent}] now equals ({gsp.score[endCurrent].x}, {gsp.score[endCurrent].y})");
                yellowScore += houseScore;
                //gHUD.ScoringPanel();
                redHammer = true;
                gHUD.SetHammer(redHammer);
            }
            else
            {
                Debug.LogWarning($"[GameManager.Scoring] winningTeamName '{winningTeamName}' doesn't match redTeamName '{redTeamName}' or yellowTeamName '{yellowTeamName}'. Falling back to red team score update to avoid losing score state.");
                gsp.score[endCurrent] = new Vector2Int(houseScore, 0);
                redScore += houseScore;
                redHammer = false;
                gHUD.SetHammer(redHammer);
            }

            //endScoreRed[endCurrent] = redScore;
            //endScoreYellow[endCurrent] = yellowScore;

            yield return new WaitForSeconds(1.5f);

        }
        else if (houseList.Count == 0)
        {
            // ? CRITICAL FIX: Save blank end score!
            // Ensure score array is big enough
            if (gsp.score == null || endCurrent >= gsp.score.Length)
            {
                Debug.LogWarning($"[GameManager.Scoring] Score array too small for blank end - resizing");
                int newSize = Mathf.Max(endTotal, endCurrent + 1);
                Vector2Int[] newScore = new Vector2Int[newSize];
                
                if (gsp.score != null)
                {
                    for (int i = 0; i < gsp.score.Length; i++)
                    {
                        newScore[i] = gsp.score[i];
                    }
                }
                
                gsp.score = newScore;
            }
            
            // Save blank end
            gsp.score[endCurrent] = new Vector2Int(0, 0);
            Debug.Log($"[GameManager.Scoring] SAVED blank end to gsp.score[{endCurrent}] = (0, 0)");
            
            if (redHammer)
            {
                redHammer = true;
                //gHUD.ScoringPanel();
                gHUD.ScoringUI(redTeamName, " ", 0);
                gHUD.SetHammer(redHammer);
            }
            else
            {
                redHammer = false;
                //gHUD.ScoringPanel();
                gHUD.ScoringUI(yellowTeamName, " ", 0);
                gHUD.SetHammer(redHammer);
            }

        }

        rockBar.EndUpdate(yellowScore, redScore);
        
        // Wait for player to acknowledge scoring before continuing to next end
        yield return StartCoroutine(WaitForClick());

        if (endCurrent < endTotal)
        {
            gHUD.ScoreboardOff();
            gHUD.MainDisplayOff();
            state = GameState.RESET;

            rockCurrent = gsp.rockCurrent;
            endCurrent++;
            gsp.endCurrent = endCurrent;
            gsp.LoadFromGM();
            
            // CRITICAL FIX: Clear rock positions when end finishes
            // These rocks belong to the completed end, not the upcoming end
            // When player continues, they'll start fresh with no rocks to restore
            gsp.rockPos = null;
            gsp.rockInPlay = null;
            // CRITICAL: Reset rockCurrent for next end
            int firstPlayerRock = 16 - (gsp.rocks * 2);
            gsp.rockCurrent = firstPlayerRock;
            Debug.Log($"[GameManager] Cleared rockPos and reset rockCurrent={gsp.rockCurrent} for end transition");
            
            // CRITICAL FIX: When end finishes (not game), keep gameInProgress = true for next end
            gsp.gameInProgress = true;
            Debug.Log("[GameManager] End finished - gameInProgress remains true for next end");
            
            SaveGame();
            yield return new WaitForEndOfFrame();
            SceneManager.LoadScene("End_Menu_Tourny_1");
        }
        else if (endCurrent >= endTotal)
        {
            state = GameState.END;
            StartCoroutine(EndOfGame());
        }
    }

    IEnumerator EndOfGame()
    {
        gHUD.EndOfGame(redScore, redTeamName, yellowScore, yellowTeamName);
        
        yield return new WaitForSeconds(2f);

        endCurrent++;

        gsp.LoadFromGM();
        
        // CRITICAL FIX: Clear rock positions when game ends
        gsp.rockPos = null;
        gsp.rockInPlay = null;
        gsp.rockCurrent = 0; // Reset for potential replay
        
        // CRITICAL FIX: Clear game state when game ends
        gsp.loadGame = false;
        gsp.gameInProgress = false;
        Debug.Log("[GameManager] Game ended - cleared gameInProgress, loadGame flags, and rockPos");
        Debug.Log($"[GameManager] Final score: {redTeamName} {redScore} - {yellowTeamName} {yellowScore}");
        
        SaveGame();
        SceneManager.LoadScene("End_Menu_Tourny_1");
    }
    #endregion

    #region Utilities and Special Situations
    IEnumerator WaitForClick()
    {
        // Wait for either click OR 5 seconds timeout
        float elapsed = 0f;
        while (elapsed < 5f && !Input.GetMouseButtonDown(0))
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[WaitForClick] Clicked to advance");
        }
        else
        {
            Debug.Log("[WaitForClick] Auto-advanced after 5 seconds");
        }
    }

    public void OnDebug()
    {
        db.SetActive(false);

        GetComponent<Debug_Placement>().enabled = true;
        GetComponent<Debug_Shooting>().enabled = true;
        GetComponent<Debug_Random>().enabled = true;

        dbrandom.SetActive(true);
    }

    public void OnDebugReset()
    {
        StartCoroutine(Scoring());
    }

    void SaveGame()
    {
        gsp.AutoSave();
    }

    IEnumerator LoadGame()
    {
        Debug.Log("Loading Game!!!!!!!");
        
        redHammer = gsp.redHammer;
        endTotal = gsp.ends;
        endCurrent = gsp.endCurrent;
        rockCurrent = gsp.rockCurrent;
        //rockTotal = myFile.GetInt("Rocks Per Team") * 2;
        aiTeamRed = gsp.aiRed;
        aiTeamYellow = gsp.aiYellow;
        
        redScore = gsp.redScore;
        yellowScore = gsp.yellowScore;
        rocksPerTeam = gsp.rocks;

        // ? REMOVED: Score array is already loaded by CareerManager.RestoreGameState()!
        // This code was creating EMPTY arrays and overwriting the loaded scores with zeros!
        // DO NOT reload scores here - they're already correct in gsp.score[]
        
        Debug.Log($"[GameManager.LoadGame] Score array preserved from save: End1=({gsp.score[0].x},{gsp.score[0].y}), End2=({(gsp.score.Length > 1 ? gsp.score[1].x : -1)},{(gsp.score.Length > 1 ? gsp.score[1].y : -1)})");

        for (int i = 1; i < endCurrent; i++)
        {
            gHUD.ScoringPanel();
        }

        yield return StartCoroutine(SetupRocks());

        yield return new WaitForEndOfFrame();

        rockBar.ResetBar(redHammer);
        rockBar.EndUpdate(yellowScore, redScore);
        
        // When loading, always use the saved rockCurrent value
        // It was already set correctly by SetupGame (new game) or CheckScore (new end)
        rockCurrent = gsp.rockCurrent;
        Debug.Log($"[GameManager.LoadGame] Restored rockCurrent from save: {rockCurrent}");
        Debug.Log($"[GameManager.LoadGame] This means rocks 0-{rockCurrent} were already played. Next rock will be {rockCurrent + 1}");

        //yield return StartCoroutine(WaitForClick());
        //gsp.loadGame = false;
        yield return new WaitUntil(() => rockBar.rockListUI.Count == 16);

        // Show intro for tournament games on the first played game in the tournament
        // This handles loading saved games where intro hasn't been shown yet
        if (!gsp.tournyIntroShown && !gsp.cashGame && !gsp.debug)
        {
            yield return StartCoroutine(TournyIntro());
            gsp.tournyIntroShown = true;  // Mark intro as shown
            Debug.Log("[GameManager] Tourny intro shown and flag set (loaded game)");
        }

        // Show announcer dialogue at the START of the end (before placing rocks)
        // Skip on end 1 (endCurrent == 0) as that's the very first end with no score yet
        // This covers the "continue from End Menu" flow
        if (endCurrent >= 1 && !gsp.cashGame && !gsp.debug && (gsp.rockPos == null || gsp.rockPos.Length == 0))
        {
            DialogueData selectedDialogue = SelectDialogueBasedOnGameState();
            if (selectedDialogue != null && DialogueController.Instance != null)
            {
                // Enable dialogue canvas before showing
                if (dialogueCanvas != null)
                    dialogueCanvas.enabled = true;
                    
                DialogueController.Instance.Show(selectedDialogue);
                // Wait for dialogue to complete before continuing
                yield return new WaitUntil(() => !DialogueController.Instance.IsShowing);
                Debug.Log("[GameManager] Announcer dialogue shown at start of loaded end " + endCurrent);
            }
        }

        // CRITICAL FIX: Only place rocks if we have positions to restore
        // When continuing from End Menu to start a fresh end, rockPos will be null/empty
        if (rockCurrent >= 0 && gsp.rockPos != null && gsp.rockPos.Length > 0)
        {
            yield return StartCoroutine(PlaceRocks());
            
            // After restoring rocks, start the next turn directly
            // Don't call CheckScore - rocks are already in final positions
            Debug.Log($"[GameManager.LoadGame] Rocks restored, starting next turn (rock {rockCurrent + 1})");
            NextTurn();
            yield break; // Don't call CheckScore
        }
        else if (rockCurrent > 0 && (gsp.rockPos == null || gsp.rockPos.Length == 0))
        {
            // CRITICAL FIX: Fresh end - need to place rocks 0 through rockCurrent-1
            // This simulates the first N rocks based on gsp.rocks setting
            Debug.Log($"[GameManager.LoadGame] Fresh end - placing rocks 0 to {rockCurrent - 1} (gsp.rocks={gsp.rocks})");
            
            cm.HouseView();
            rockBar.ResetBar(redHammer);
            
            // Place all simulated rocks
            for (int i = 0; i < rockCurrent; i++)
            {
                rm.rrp.placed1 = false;
                if (gsp.aiRed)
                {
                    yield return rm.rrp.OnRockPlace(i, false);
                }
                else if (gsp.aiYellow)
                {
                    yield return rm.rrp.OnRockPlace(i, true);
                }

                if (i % 2 == 0)
                {
                    if (redHammer)
                        rockBar.ActiveRock(false, i);
                    else
                        rockBar.ActiveRock(true, i);
                }
                else if (i % 2 == 1)
                {
                    if (redHammer)
                        rockBar.ActiveRock(true, i);
                    else
                        rockBar.ActiveRock(false, i);
                }
            }
            
            rockCurrent--; // CRITICAL: Decrement so NextTurn() increments to correct first rock
            rm.rrp.placed = true;
            Debug.Log($"[GameManager.LoadGame] Completed placing {rockCurrent + 1} rocks, decremented rockCurrent to {rockCurrent}");
        }
        else if (rockCurrent == 0)
        {
            // Fresh end with no sims - rockCurrent is 0, start first turn directly
            Debug.Log($"[GameManager.LoadGame] Fresh end, no sims - starting first turn directly");
            rockBar.ResetBar(redHammer);
            rockBar.EndUpdate(yellowScore, redScore);
            yield return new WaitUntil(() => rockBar.rockListUI.Count == 16);
            rm.rrp.placed = true;
            if (redHammer)
                OnYellowTurn();
            else
                OnRedTurn();
            yield break; // Don't call CheckScore
        }
            

        //myFile.Dispose();


        yield return StartCoroutine(CheckScore());
    }

    IEnumerator StoryLoad()
    {
        Debug.Log("Story Game!!!!!!!");
        redHammer = storyM.redHammer;
        endTotal = storyM.ends;
        endCurrent = storyM.endCurrent;
        rockTotal = storyM.rocks * 2;
        aiTeamRed = storyM.aiRed;
        aiTeamYellow = storyM.aiYellow;

        //gHUD.Scoreboard(3, 3, 0);
        //gHUD.Scoreboard(4, 0, 2);
        //gHUD.Scoreboard(6, 5, 0);
        //gHUD.Scoreboard(8, 6, 0);

        redScore = storyM.redScore;
        yellowScore = storyM.yellowScore;
        rockBar.EndUpdate(yellowScore, redScore);

        yield return StartCoroutine(SetupRocks());

        rockBar.ResetBar(redHammer);
        yield return new WaitForEndOfFrame();
            rockCurrent = storyM.rockCurrent - 1;

        Debug.Log("Current Rock is " + rockCurrent);
        for (int i = 0; i <= rockCurrent; i++)
        {
            rockList[i].rockInfo.placed = true;
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i <= rockCurrent; i++)
        {
            rockList[i].rock.GetComponent<CircleCollider2D>().radius = 0.14f;
            rockList[i].rock.GetComponent<SpriteRenderer>().enabled = true;
            rockList[i].rock.GetComponent<SpringJoint2D>().enabled = false;
            rockList[i].rock.GetComponent<Rock_Flick>().enabled = false;
            rockList[i].rock.transform.parent = null;
            //rockBar.DeadRock(i);
            yield return new WaitForEndOfFrame();
        }

        for (int i = 0; i <= rockCurrent; i++)
        {
            Vector2 rockTrans = storyM.rockPos[i];
            Debug.Log("Rock Position " + i + " " + rockTrans.x + ", " + rockTrans.y);
            rockList[i].rock.GetComponent<Rigidbody2D>().position = rockTrans;

            if (storyM.rockPos[i].y >= 8f)
            {
                rockList[i].rock.SetActive(false);
                rockList[i].rockInfo.inPlay = false;
                rockList[i].rockInfo.outOfPlay = true;
                //rockBar.DeadRock(i);
            }
            else
            {
                rockList[i].rock.GetComponent<CircleCollider2D>().enabled = true;
                rockList[i].rock.GetComponent<Rock_Release>().enabled = true;
                rockList[i].rock.GetComponent<Rock_Force>().enabled = true;
                rockList[i].rock.GetComponent<Rock_Colliders>().enabled = true;
                rockList[i].rockInfo.inPlay = true;
                rockList[i].rockInfo.outOfPlay = false;
                rockList[i].rockInfo.moving = false;
                rockList[i].rockInfo.shotTaken = true;
                rockList[i].rockInfo.released = true;
                rockList[i].rockInfo.stopped = true;
                rockList[i].rockInfo.rest = true;
                Debug.Log("i is equal to " + i);

                yield return new WaitForEndOfFrame();
            }
            
        }

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < rockCurrent; i++)
        {

        }
        //rocksPlaced = true;

        yield return new WaitForEndOfFrame();

        yield return StartCoroutine(CheckScore());
    }

    IEnumerator PlaceRocks()
    {
        // SAFETY CHECKS
        if (gsp.rockPos == null || gsp.rockPos.Length == 0)
        {
            Debug.LogError("[GameManager] PlaceRocks() - gsp.rockPos is NULL or empty! Cannot place rocks!");
            yield break;
        }
        
        if (gsp.rockInPlay == null || gsp.rockInPlay.Length == 0)
        {
            Debug.LogError("[GameManager] PlaceRocks() - gsp.rockInPlay is NULL or empty! Cannot place rocks!");
            yield break;
        }
        
        Debug.Log($"[GameManager] PlaceRocks() - Restoring {rockCurrent + 1} rocks from save");
        Debug.Log($"[GameManager] gsp.loadGame = {gsp.loadGame}");
        Debug.Log($"[GameManager] gsp.rockPos.Length = {gsp.rockPos.Length}");
        Debug.Log($"[GameManager] gsp.rockInPlay.Length = {gsp.rockInPlay.Length}");
        Debug.Log($"[GameManager] rockList.Count = {rockList.Count}");

        // Mark rocks as placed
        for (int i = 0; i <= rockCurrent; i++)
        {
            if (i < rockList.Count)
            {
                rockList[i].rockInfo.placed = true;
            }
        }

        yield return new WaitForEndOfFrame();

        // Configure rocks and restore positions
        for (int i = 0; i <= rockCurrent; i++)
        {
            if (i >= rockList.Count)
            {
                Debug.LogError($"[GameManager] Rock index {i} out of bounds (rockList.Count = {rockList.Count})");
                continue;
            }

            // Basic rock setup
            rockList[i].rock.GetComponent<CircleCollider2D>().radius = 0.14f;
            rockList[i].rock.GetComponent<SpringJoint2D>().enabled = false;
            rockList[i].rock.GetComponent<Rock_Flick>().enabled = false;
            rockList[i].rock.transform.parent = null;
            
            yield return new WaitForEndOfFrame();

            // CRITICAL: Restore position from save data
            if (gsp.loadGame && i < gsp.rockInPlay.Length && i < gsp.rockPos.Length)
            {
                if (gsp.rockInPlay[i])
                {
                    // Rock is IN PLAY - restore position
                    Vector2 rockTrans = gsp.rockPos[i];
                    Debug.Log($"[GameManager] Restoring Rock {i}: pos=({rockTrans.x:F2}, {rockTrans.y:F2}), inPlay=true");
                    
                    rockList[i].rock.GetComponent<Rigidbody2D>().position = rockTrans;
                    rockList[i].rock.GetComponent<SpriteRenderer>().enabled = true;
                    rockList[i].rock.GetComponent<CircleCollider2D>().enabled = true;
                    rockList[i].rock.GetComponent<Rock_Release>().enabled = true;
                    rockList[i].rock.GetComponent<Rock_Force>().enabled = true;
                    rockList[i].rock.GetComponent<Rock_Colliders>().enabled = true;
                    
                    rockList[i].rockInfo.inPlay = true;
                    rockList[i].rockInfo.outOfPlay = false;
                    rockList[i].rockInfo.moving = false;
                    rockList[i].rockInfo.shotTaken = true;
                    rockList[i].rockInfo.released = true;
                    rockList[i].rockInfo.stopped = true;
                    rockList[i].rockInfo.rest = true;
                    
                    // Update rock bar UI - rock is idle (played but in play)
                    rockBar.IdleRock(i);
                }
                else
                {
                    // Rock is OUT OF PLAY - hide it
                    Debug.Log($"[GameManager] Rock {i} is OUT OF PLAY - hiding");
                    rockList[i].rock.SetActive(false);
                    rockList[i].rockInfo.inPlay = false;
                    rockList[i].rockInfo.outOfPlay = true;
                    
                    // Update rock bar UI - rock is dead (out of play)
                    rockBar.DeadRock(i);
                }
            }
            else
            {
                // No save data or index out of range - mark as out of play
                Debug.LogWarning($"[GameManager] No save data for rock {i} - marking as out of play");
                rockList[i].rock.SetActive(false);
                rockList[i].rockInfo.inPlay = false;
                rockList[i].rockInfo.outOfPlay = true;
            }

            yield return new WaitForEndOfFrame();
        }

        yield return new WaitForEndOfFrame();
        rm.rrp.placed = true;
        
        Debug.Log("[GameManager] PlaceRocks() complete - all rocks restored");
    }

    IEnumerator TournyIntro()
    {
        CareerManager careerM = FindFirstObjectByType<CareerManager>();
        if (careerM != null)
            Debug.Log("ID is " + careerM.currentTourny.id);

        // Check if tourny intro components exist
        if (tournyIntroGO == null)
        {
            Debug.LogWarning("[TournyIntro] tournyIntroGO is null, skipping intro");
            yield break;
        }
        
        tournyIntroGO.SetActive(true);

        // Only set animator if it exists and has a controller
        if (tournyIntro != null && tournyIntro.runtimeAnimatorController != null)
        {
            if (careerM.currentTourny.id < 50 | careerM.currentTourny.id == 100 | careerM.currentTourny.id == 101)
            {
                tournyIntro.SetInteger("Tourny", careerM.currentTourny.id);
                yield return new WaitForEndOfFrame();
            }
            else if (careerM.currentTourny.id >= 50 & careerM.currentTourny.id < 54)
            {
                tournyIntro.SetInteger("Tourny", 50);
                yield return new WaitForEndOfFrame();
            }
        }
        else
        {
            Debug.LogWarning("[TournyIntro] Animator is null or has no controller, showing intro without animation");
            // Show intro without animation, or just skip it
            if (careerM.currentTourny.id >= 50 && careerM.currentTourny.id < 100)
            {
                tournyIntroGO.SetActive(false);
            }
        }
        
        // Wait for 6 seconds OR click to skip
        float elapsed = 0f;
        while (elapsed < 6f && !Input.GetMouseButtonDown(0))
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("[TournyIntro] Skipped by click at " + elapsed.ToString("F2") + "s");
        }

        tournyIntroGO.SetActive(false);
        
        // Show tournament intro dialogue
        if (DialogueController.Instance != null)
        {
            DialogueData introToShow = null;
            
            // Try to use tournament-specific intro dialogue first
            if (careerM.currentTourny != null && careerM.currentTourny.introDialogue != null)
            {
                introToShow = careerM.currentTourny.introDialogue;
                Debug.Log($"[GameManager] Showing tournament-specific intro for {careerM.currentTourny.name}");
            }
            // Fallback to generic Coach_Welcome if no tournament intro assigned
            else if (Coach_Welcome != null)
            {
                introToShow = Coach_Welcome;
                Debug.Log("[GameManager] No tournament intro assigned, using Coach_Welcome");
            }
            
            // Show the dialogue and wait for it to complete
            if (introToShow != null)
            {
                // Enable dialogue canvas before showing
                if (dialogueCanvas != null)
                    dialogueCanvas.enabled = true;
                    
                DialogueController.Instance.Show(introToShow);
                yield return new WaitUntil(() => !DialogueController.Instance.IsShowing);
                Debug.Log("[GameManager] Intro dialogue completed");
            }
            else
            {
                Debug.LogWarning("[GameManager] No intro dialogue available (neither tournament intro nor Coach_Welcome assigned)");
            }
        }
        else
        {
            Debug.LogError("[GameManager] DialogueController not found in scene! Add a GameObject with DialogueController component.");
        }
    }
    
    /// <summary>
    /// Selects dialogue based on current game state (winning/losing, margin, timing)
    /// </summary>
    private DialogueData SelectDialogueBasedOnGameState()
    {
        // Determine if player is red or yellow
        bool playerIsRed = !aiTeamRed && aiTeamYellow;
        int playerScore = playerIsRed ? redScore : yellowScore;
        int opponentScore = playerIsRed ? yellowScore : redScore;
        int scoreDifference = playerScore - opponentScore;
        
        // Define "late game" as more than halfway through total ends
        bool isLateGame = endCurrent > (endTotal / 2);
        
        // Select dialogue based on score and timing
        if (scoreDifference > 0)
        {
            // Player is winning
            if (scoreDifference == 1)
            {
                // Winning by 1
                if (isLateGame && Announcer_Winning_By_1_Late != null && Announcer_Winning_By_1_Late.Length > 0)
                {
                    return Announcer_Winning_By_1_Late[Random.Range(0, Announcer_Winning_By_1_Late.Length)];
                }
                else if (!isLateGame && Announcer_Winning_By_1 != null && Announcer_Winning_By_1.Length > 0)
                {
                    return Announcer_Winning_By_1[Random.Range(0, Announcer_Winning_By_1.Length)];
                }
            }
            else
            {
                // Winning by 2+
                if (isLateGame && Announcer_Winning_By_Many_Late != null && Announcer_Winning_By_Many_Late.Length > 0)
                {
                    return Announcer_Winning_By_Many_Late[Random.Range(0, Announcer_Winning_By_Many_Late.Length)];
                }
                else if (!isLateGame && Announcer_Winning_By_Many != null && Announcer_Winning_By_Many.Length > 0)
                {
                    return Announcer_Winning_By_Many[Random.Range(0, Announcer_Winning_By_Many.Length)];
                }
            }
        }
        else if (scoreDifference < 0)
        {
            // Player is losing
            int deficit = Mathf.Abs(scoreDifference);
            
            if (deficit == 1)
            {
                // Losing by 1
                if (isLateGame && Announcer_Losing_By_1_Late != null && Announcer_Losing_By_1_Late.Length > 0)
                {
                    return Announcer_Losing_By_1_Late[Random.Range(0, Announcer_Losing_By_1_Late.Length)];
                }
                else if (!isLateGame && Announcer_Losing_By_1 != null && Announcer_Losing_By_1.Length > 0)
                {
                    return Announcer_Losing_By_1[Random.Range(0, Announcer_Losing_By_1.Length)];
                }
            }
            else
            {
                // Losing by 2+
                if (isLateGame && Announcer_Losing_By_Many_Late != null && Announcer_Losing_By_Many_Late.Length > 0)
                {
                    return Announcer_Losing_By_Many_Late[Random.Range(0, Announcer_Losing_By_Many_Late.Length)];
                }
                else if (!isLateGame && Announcer_Losing_By_Many != null && Announcer_Losing_By_Many.Length > 0)
                {
                    return Announcer_Losing_By_Many[Random.Range(0, Announcer_Losing_By_Many.Length)];
                }
            }
        }
        
        // Tied or no appropriate dialogue - return null to use fallback
        return null;
    }
    #endregion
}
