using System;
using UnityEngine;

/// <summary>
/// Defines the setup for the tutorial game.
/// This is used on the first career game to teach basic mechanics.
/// </summary>
[CreateAssetMenu(fileName = "TutorialGameSetup", menuName = "Tutorial/Game Setup")]
public class TutorialGameSetup : ScriptableObject
{
    [Header("Game State")]
    [Tooltip("Which end we're starting in")]
    public int startingEnd = 2;
    
    [Tooltip("Score at tutorial start (opponent-player)")]
    public Vector2Int startingScore = new Vector2Int(1, 0);
    
    [Tooltip("Does player have hammer?")]
    public bool playerHasHammer = true;
    
    [Tooltip("Rocks per team")]
    public int rocksPerTeam = 2;
    
    [Header("Team Setup")]
    [Tooltip("Opponent team name for tutorial")]
    public string opponentTeamName = "Tutorial Opponent";
    
    [Header("Pre-placed Rocks")]
    [Tooltip("Rocks that are already on the ice when tutorial starts")]
    public PrePlacedRock[] prePlacedRocks;
    
    [Header("Tutorial Triggers")]
    [Tooltip("Tutorial sequences to fire as the player reaches specific rocks during the tutorial game")]
    public TutorialTrigger[] tutorialTriggers;

    [Header("AI Behavior (Optional Overrides)")]
    [Tooltip("Suggest AI shot types for specific rocks (leave empty for normal AI)")]
    public AIShotSuggestion[] aiShotSuggestions;
}

[Serializable]
public class TutorialTrigger
{
    [Tooltip("Tutorial sequence ID to play (must match a TutorialSequence.sequenceId)")]
    public string tutorialId;

    [Tooltip("Which rock index triggers this tutorial")]
    public int rockIndex;

    [Tooltip("If true, ignore rockIndex and trigger when the player reaches their first rock this end. " +
             "Safety net for when hammer/AI flags shift which rock index is actually first.")]
    public bool triggerOnFirstPlayerRock;
}

[Serializable]
public class PrePlacedRock
{
    [Tooltip("Which rock in the list (0-15)")]
    public int rockIndex;
    
    [Tooltip("Is this a yellow team rock?")]
    public bool isYellow;
    
    [Tooltip("World position to place the rock")]
    public Vector2 position;
    
    [Tooltip("Optional: Set a specific rotation")]
    public float rotation = 0f;
}

[Serializable]
public class AIShotSuggestion
{
    [Tooltip("Which rock index this applies to")]
    public int rockIndex;
    
    [Tooltip("Suggested shot type")]
    public AIShotType suggestedShotType;
    
    [Tooltip("If takeout, suggest hitting this specific rock index")]
    public int targetRockIndex = -1;
    
    [Tooltip("Allow AI to use runback? (might miss)")]
    public bool allowRunback = false;
}

public enum AIShotType
{
    Normal,        // Let AI decide
    DirectHit,     // Direct takeout
    Draw,          // Draw shot
    Guard          // Place a guard
}
