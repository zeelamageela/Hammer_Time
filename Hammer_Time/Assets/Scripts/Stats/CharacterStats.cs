using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterStats : MonoBehaviour
{
    Sweeper sweeper;

    public string charName;
    public Stat accuracy;
    public Stat sweepStrength;
    public Stat sweepEndurance;
    public Stat weight;


    public float sweepHealth;
    public bool sweeping;
    public bool sweepingHard;
    public bool shooting;

    private void Start()
    {

        sweeper = GetComponent<Sweeper>();
    }

    private void Update()
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
    public void OnSweepFatigue(float fatigue)
    {
        fatigue = fatigue - (sweepEndurance.GetValue() / 10f);

        sweepHealth -= (fatigue / 2f);

        if (sweepHealth <= 0)
        {
            sweeper.Whoa();
        }
    }

    public void OnSweepRecover()
    {
        if (sweepHealth < 100)
        sweepHealth += (sweepEndurance.GetValue()) * 0.05f;
    }
}
