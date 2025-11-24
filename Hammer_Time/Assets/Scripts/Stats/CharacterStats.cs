using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    SweeperParent sweeper;

    AIManager aim;

    public string charName;
    public Stat drawAccuracy;
    public Stat takeOutAccuracy;
    public Stat guardAccuracy;
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
        if (!GetComponent<SweeperParent>())
        {
            OnShoot();
        }
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
        fatigue -= 0.01f * (100f - sweepEndurance.GetValue());
        Debug.Log("Fatigue is " + fatigue);
        sweepHealth -= fatigue;
        Debug.Log("Sweep Health is " + fatigue);

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

        Debug.Log("AI Take Out Accuracy is " + takeOutAccuracy.GetValue());

        aiShoot.drawAccu = new Vector2(0.1f - (0.001f * drawAccuracy.GetValue()), 0.1f - (0.001f * drawAccuracy.GetValue()));
        aiShoot.guardAccu = new Vector2(0.1f - (0.001f * guardAccuracy.GetValue()), 0.1f - (0.001f * guardAccuracy.GetValue()));
        aiShoot.toAccu = new Vector2(0.1f - (0.001f * takeOutAccuracy.GetValue()), 0.1f - (0.001f * takeOutAccuracy.GetValue()));
    }
}