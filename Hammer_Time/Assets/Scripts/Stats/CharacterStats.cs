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
    [Tooltip("Worn out: no sweeping until some energy is back (SweepFatigue.ReadyAgain).")]
    public bool spent;
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
        sweepMax = SweepFatigue.Full;
        spent = false;
        // Don't call OnShoot() here - it should be called explicitly when the character is shooting
    }

    private void Update()
    {
        if (GetComponent<SweeperParent>())
        {
            if (sweeper.sweep)
            {
                OnSweepFatigue(false);
            }
            else if (sweeper.hard)
            {
                OnSweepFatigue(true);
            }
            else if (sweeper.whoa)
            {
                OnSweepRecover();
            }
        }
    }
    public void OnSweepFatigue(bool hard)
    {
        float run = SweepFatigue.RunSeconds(RockPhysicsSettings.Current);
        float lasts = SweepFatigue.SweepSeconds(sweepEndurance.GetValue(), hard, run);
        sweepHealth -= sweepMax * Time.deltaTime / lasts;

        if (sweepHealth <= 0)
        {
            sweepHealth = 0f;
            spent = true;
            sweeper.Whoa();
        }
    }

    public void OnSweepRecover()
    {
        if (sweepHealth < sweepMax)
        {
            float run = SweepFatigue.RunSeconds(RockPhysicsSettings.Current);
            float rest = SweepFatigue.RestSeconds(sweepEndurance.GetValue(), run);
            sweepHealth = Mathf.Min(sweepMax, sweepHealth + sweepMax * Time.deltaTime / rest);
        }
        if (spent && sweepHealth >= SweepFatigue.ReadyAgain) spent = false;
    }

    public void OnShoot()
    {
        aim = FindFirstObjectByType<AIManager>();
        AI_Shooter aiShoot = aim.gameObject.GetComponent<AI_Shooter>();

        GameLog.Info("AI Aim Accuracy is " + aimAccuracy.GetValue());

        // Legacy AI_Shooter compatibility (will be removed later)
        aiShoot.drawAccu = new Vector2(0.1f - (0.001f * weightAccuracy.GetValue()), 0.1f - (0.001f * weightAccuracy.GetValue()));
        aiShoot.guardAccu = new Vector2(0.1f - (0.001f * finesseAccuracy.GetValue()), 0.1f - (0.001f * finesseAccuracy.GetValue()));
        aiShoot.toAccu = new Vector2(0.1f - (0.001f * aimAccuracy.GetValue()), 0.1f - (0.001f * aimAccuracy.GetValue()));
    }
}