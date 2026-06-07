using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Linq;

public class MainMenu : MonoBehaviour
{
    public AudioManager am;
    public GameSettingsPersist gsp;

    [Header("High Score Display")]
    public Text topPlayerName;
    public Text topPlayerEarnings;

    [Header("Trophy Display")]
    public Image[] trophyImgList;
    
    [Header("Trophy Colors")]
    public Color trophyWonColor = Color.white;
    public Color trophyLockedColor = new Color(1f, 1f, 1f, 0.3f);

    void Start()
    {
        // Check if save exists and enable/disable Continue button
        // Access save service directly (CareerManager doesn't exist in splash scene)
        // if (CareerSaveService.SaveExists())
        // {
        //     contButton.SetActive(true);
        //     Debug.Log("[MainMenu] Save file found - Continue button enabled");
        // }
        // else
        // {
            Debug.Log("[MainMenu] No save file - Continue button disabled");
        // }

        DisplayHighScores();

        am.PlayBG(0);
    }
    
    public void DisplayHighScores()
    {
        // Get top entry (highest earnings)
        List<HighScoreEntry> topScores = HighScoreService.GetTopEntries(1);
        
        if (topScores == null || topScores.Count == 0)
        {
            Debug.Log("[MainMenu] No high scores found");
            if (topPlayerName != null)
                topPlayerName.text = "No records yet";
            if (topPlayerEarnings != null)
                topPlayerEarnings.text = "$0";
            
            // Display all trophies as locked
            List<bool> emptyTrophies = new List<bool>();
            DisplayAllTimeTrophies(emptyTrophies);
            return;
        }

        HighScoreEntry topEntry = topScores[0];
        
        // Display top player name and team
        if (topPlayerName != null)
            topPlayerName.text = $"{topEntry.playerName} {topEntry.teamName}";
        
        // Display top earnings
        if (topPlayerEarnings != null)
            topPlayerEarnings.text = $"${topEntry.earnings:N0}";
        
        // Display all-time trophies (across all careers)
        List<bool> allTimeTrophies = HighScoreService.GetAllTimeTrophies();
        DisplayAllTimeTrophies(allTimeTrophies);
        
        Debug.Log($"[MainMenu] Top score: {topEntry.playerName} - ${topEntry.earnings:N0}");
    }
    
    private void DisplayAllTimeTrophies(List<bool> trophies)
    {
        if (trophyImgList == null || trophyImgList.Length == 0)
        {
            Debug.LogWarning("[MainMenu] Trophy image list not assigned");
            return;
        }
        
        if (trophies == null || trophies.Count == 0)
        {
            Debug.LogWarning("[MainMenu] No trophy data found");
            // Set all trophies to locked state (faded)
            for (int i = 0; i < trophyImgList.Length; i++)
            {
                if (trophyImgList[i] != null)
                {
                    trophyImgList[i].color = trophyLockedColor;
                }
            }
            return;
        }
        
        // Display each trophy - change color based on won/locked status
        for (int i = 0; i < trophyImgList.Length; i++)
        {
            if (trophyImgList[i] == null)
                continue;
            
            if (i < trophies.Count && trophies[i])
            {
                // Trophy won - show at full color
                trophyImgList[i].color = trophyWonColor;
            }
            else
            {
                // Trophy not won - show faded/locked
                trophyImgList[i].color = trophyLockedColor;
            }
        }
        
        int trophiesWon = trophies.Count(t => t);
        Debug.Log($"[MainMenu] All-time trophies: {trophiesWon}/{trophies.Count} won");
    }
    
    /// <summary>
    /// Call this to refresh the high score display (e.g., after returning from a completed career)
    /// </summary>
    public void RefreshHighScores()
    {
        DisplayHighScores();
    }

    public void PlayGame()
    {
        am = Object.FindAnyObjectByType<AudioManager>();
        SceneManager.LoadScene("AIGame");
        if (am != null)
            am.Play("Theme");
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }

    public void Tournament()
    {
        SceneManager.LoadScene("Career_Menu");
    }
    public void Tutorial()
    {
        if (am == null)
            am = Object.FindAnyObjectByType<AudioManager>();

        SceneManager.LoadScene("TutorialGame");
        if (am != null)
            am.Play("Theme");
    }

    public void AIGame()
    {
        am = Object.FindAnyObjectByType<AudioManager>();
        SceneManager.LoadScene("AIGame_2");
        if (am != null)
            am.Play("Theme");
    }

    public void Continue()
    {
        // CRITICAL FIX: Set careerLoad flag to trigger auto-load in CareerSettings
        Debug.Log("[MainMenu] Continue clicked - setting careerLoad flag and loading Career Menu");
        
        if (gsp != null)
        {
            gsp.careerLoad = true; // Signal to CareerSettings to auto-load
        }
        
        am = Object.FindAnyObjectByType<AudioManager>();
        if (am != null)
        {
            am.Play("Theme");
        }
        
        // Load career menu which will auto-load the correct scene via CareerSettings.Start()
        SceneManager.LoadScene("Career_Menu");
    }
}
