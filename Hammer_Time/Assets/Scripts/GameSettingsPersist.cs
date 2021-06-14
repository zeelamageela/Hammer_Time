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
        LoadSettings();
    }

    public void LoadSettings()
    {
        gs = GameObject.FindGameObjectWithTag("GameSettings").GetComponent<GameSettings>();
    }
    private void Update()
    {
        ends = gs.ends;
        rocks = gs.rocks;
        volume = gs.volume;
        redHammer = gs.redHammer;
    }
}
