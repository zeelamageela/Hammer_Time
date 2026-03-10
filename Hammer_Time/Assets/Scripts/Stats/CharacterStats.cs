using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    SweeperParent sweeper;

    AIManager aim;

    public string charName;
    public Stat weightAccuracy;   // Y-axis accuracy (distance/weight control)
    public Stat aimAccuracy;       // X-axis accuracy (lateral positioning)
    public Stat finesseAccuracy;   // Complex shot bonus (finesse techniques)
    public Stat sweepStrength;
    public Stat sweepEndurance;
    public Stat sweepCohesion;


    public float sweepHealth;
    float sweepMax;
    public bool sweeping;
    public bool sweepingHard;
    public bool shooting;

    private void Start()
    {
        sweeper = GetComponent<SweeperParent>();
    }

    private void OnEnable()
    {
        sweepHealth = 100f;
        sweepMax = 100f;
        // Don't call OnShoot() here - it should be called explicitly when the character is shooting
    }

    private void Update()
    {
        if (GetComponent<SweeperParent>())
        {
            if (sweeper.sweep)
            {
                OnSweepFatigue(1);
            }
            else if (sweeper.hard)
            {
                OnSweepFatigue(2);
            }
            else if (sweeper.whoa)
            {
                OnSweepRecover();
            }
        }
    }
    public void OnSweepFatigue(float fatigue)
    {
        fatigue -= 0.005f * (100f - sweepEndurance.GetValue());
        //Debug.Log("Fatigue is " + fatigue);
        sweepHealth -= fatigue;
        //Debug.Log("Sweep Health is " + fatigue);

        if (sweepHealth <= 0)
        {
            sweepHealth = 0f;
            sweeper.Whoa();
        }
    }

    public void OnSweepRecover()
    {
        if (sweepHealth < sweepMax)
            sweepHealth += (1 - (sweepEndurance.GetValue() * 0.01f));
    }

    public void OnShoot()
    {
        aim = FindFirstObjectByType<AIManager>();
        AI_Shooter aiShoot = aim.gameObject.GetComponent<AI_Shooter>();

        Debug.Log("AI Aim Accuracy is " + aimAccuracy.GetValue());

        // Legacy AI_Shooter compatibility (will be removed later)
        aiShoot.drawAccu = new Vector2(0.1f - (0.001f * weightAccuracy.GetValue()), 0.1f - (0.001f * weightAccuracy.GetValue()));
        aiShoot.guardAccu = new Vector2(0.1f - (0.001f * finesseAccuracy.GetValue()), 0.1f - (0.001f * finesseAccuracy.GetValue()));
        aiShoot.toAccu = new Vector2(0.1f - (0.001f * aimAccuracy.GetValue()), 0.1f - (0.001f * aimAccuracy.GetValue()));
    }
}