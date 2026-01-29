using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

/// <summary>
/// High Score Service - Manages persistent all-time leaderboard
/// Independent of career saves - survives career deletion
/// Save Location: Application.persistentDataPath/high_scores.json
/// </summary>
public static class HighScoreService
{
    private const string HIGH_SCORE_FILENAME = "high_scores.json";
    private const int MAX_ENTRIES = 100;
    
    private static string SavePath => Path.Combine(Application.persistentDataPath, HIGH_SCORE_FILENAME);
    
    /// <summary>
    /// Load high scores from disk
    /// </summary>
    public static HighScoreData LoadHighScores()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                Debug.Log("[HighScoreService] No high score file found - creating new");
                return new HighScoreData();
            }
            
            string json = File.ReadAllText(SavePath);
            HighScoreData data = JsonUtility.FromJson<HighScoreData>(json);
            
            if (data == null)
            {
                Debug.LogWarning("[HighScoreService] Failed to parse high scores - creating new");
                return new HighScoreData();
            }
            
            Debug.Log($"[HighScoreService] Loaded {data.entries.Count} high score entries");
            return data;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[HighScoreService] Error loading high scores: {ex.Message}");
            return new HighScoreData();
        }
    }
    
    /// <summary>
    /// Save high scores to disk
    /// </summary>
    public static bool SaveHighScores(HighScoreData data)
    {
        try
        {
            if (data == null)
            {
                Debug.LogError("[HighScoreService] Cannot save null high score data");
                return false;
            }
            
            data.lastUpdated = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
            
            Debug.Log($"[HighScoreService] Saved {data.entries.Count} high score entries to {SavePath}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[HighScoreService] Error saving high scores: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Add a new career completion to high scores
    /// </summary>
    public static void AddCareerEntry(string playerName, string teamName, float earnings, int season, int wins, int losses, List<bool> trophies = null)
    {
        try
        {
            HighScoreData data = LoadHighScores();
            
            // Create new entry
            HighScoreEntry newEntry = new HighScoreEntry(playerName, teamName, earnings, season, wins, losses);
            data.entries.Add(newEntry);
            
            // Sort by earnings (descending)
            data.entries = data.entries.OrderByDescending(e => e.earnings).ToList();
            
            // Trim to max entries
            if (data.entries.Count > MAX_ENTRIES)
            {
                data.entries = data.entries.Take(MAX_ENTRIES).ToList();
            }
            
            // Update trophy cabinet if provided
            if (trophies != null && trophies.Count > 0)
            {
                // Initialize all-time trophies if empty
                if (data.allTimeTrophies.Count == 0)
                {
                    data.allTimeTrophies = new List<bool>(trophies);
                }
                else
                {
                    // Merge trophies - mark as won if won in any career
                    for (int i = 0; i < Mathf.Min(trophies.Count, data.allTimeTrophies.Count); i++)
                    {
                        if (trophies[i])
                        {
                            data.allTimeTrophies[i] = true;
                        }
                    }
                    
                    // Add any new trophies that weren't in the list before
                    if (trophies.Count > data.allTimeTrophies.Count)
                    {
                        for (int i = data.allTimeTrophies.Count; i < trophies.Count; i++)
                        {
                            data.allTimeTrophies.Add(trophies[i]);
                        }
                    }
                }
            }
            
            SaveHighScores(data);
            
            Debug.Log($"[HighScoreService] Added career: {playerName} {teamName} - ${earnings:N0}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[HighScoreService] Error adding career entry: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Get top N entries
    /// </summary>
    public static List<HighScoreEntry> GetTopEntries(int count = 10)
    {
        HighScoreData data = LoadHighScores();
        return data.entries.Take(count).ToList();
    }
    
    /// <summary>
    /// Get all-time trophy status
    /// </summary>
    public static List<bool> GetAllTimeTrophies()
    {
        HighScoreData data = LoadHighScores();
        return data.allTimeTrophies ?? new List<bool>();
    }
    
    /// <summary>
    /// Delete high scores (for testing or reset)
    /// </summary>
    public static void DeleteHighScores()
    {
        try
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                Debug.Log("[HighScoreService] High scores deleted");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[HighScoreService] Error deleting high scores: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Check if high scores exist
    /// </summary>
    public static bool HighScoresExist()
    {
        return File.Exists(SavePath);
    }
    
    /// <summary>
    /// Get high score file info
    /// </summary>
    public static string GetHighScoreInfo()
    {
        if (!File.Exists(SavePath))
        {
            return "No high scores yet";
        }
        
        try
        {
            HighScoreData data = LoadHighScores();
            return $"{data.entries.Count} entries, Last updated: {data.lastUpdated}";
        }
        catch
        {
            return "Error reading high scores";
        }
    }
}
