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
        //sweepHealth = 100;
        sweepMax = sweepHealth;
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
        fatigue = fatigue - (sweepEndurance.GetValue() * 0.065f);
        //Debug.Log("Fatigue is " + fatigue);
        sweepHealth -= fatigue;
        //Debug.Log("Sweep Health is " + fatigue);

        if (sweepHealth <= 0)
        {
            sweeper.Whoa();
        }
    }

    public void OnSweepRecover()
    {
        if (sweepHealth < sweepMax)
            sweepHealth += (sweepEndurance.GetValue()) * 0.05f;
    }

    public void OnShoot()
    {
        aim = FindObjectOfType<AIManager>();
        AI_Shooter aiShoot = aim.gameObject.GetComponent<AI_Shooter>();
        aiShoot.drawAccu = new Vector2(0.1f - (0.01f * drawAccuracy.GetValue()), 0.1f - (0.01f * drawAccuracy.GetValue()));
        aiShoot.guardAccu = new Vector2(0.1f - (0.01f * guardAccuracy.GetValue()), 0.1f - (0.01f * guardAccuracy.GetValue()));
        aiShoot.toAccu = new Vector2(0.1f - (0.01f * takeOutAccuracy.GetValue()), 0.1f - (0.01f * takeOutAccuracy.GetValue()));
    }
}