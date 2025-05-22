using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Player
{
    public string name;

    public int id;
    [TextArea(3, 10)]
    public string description;

    public float cost;

    public Sprite image;
    public bool active;
    public bool view;

    public int draw;
    public int guard;
    public int takeOut;
    public int sweepStrength;
    public int sweepEnduro;
    public int sweepCohesion;

    public int oppDraw;
    public int oppGuard;
    public int oppTakeOut;
    public int oppStrength;
    public int oppEnduro;
    public int oppCohesion;
    public void ScaleStatsToTotal()
    {
        int[] stats = { draw, takeOut, guard, sweepStrength, sweepEnduro, sweepCohesion };
        int originalTotal = 0;
        foreach (int s in stats)
            originalTotal += s;

        if (originalTotal == 0)
            return; // Avoid division by zero

        // Check if already scaled (all multiples of 10 and sum is multiple of 10)
        bool alreadyScaled = (originalTotal % 10 == 0);
        if (alreadyScaled)
        {
            bool allStatsAreMultiplesOf10 = true;
            for (int i = 0; i < 6; i++)
            {
                if (stats[i] % 10 != 0)
                {
                    allStatsAreMultiplesOf10 = false;
                    break;
                }
            }
            if (allStatsAreMultiplesOf10)
                return;
        }

        int targetTotal = originalTotal * 10;

        // Scale each stat proportionally so the total is targetTotal
        int[] scaled = new int[6];
        int scaledTotal = 0;
        for (int i = 0; i < 6; i++)
        {
            scaled[i] = Mathf.RoundToInt((stats[i] / (float)originalTotal) * targetTotal);
            scaledTotal += scaled[i];
        }

        // Adjust for rounding errors to ensure the sum is exactly targetTotal
        int diff = targetTotal - scaledTotal;
        if (diff != 0)
        {
            int maxIdx = 0;
            for (int i = 1; i < 6; i++)
                if (scaled[i] > scaled[maxIdx]) maxIdx = i;
            scaled[maxIdx] += diff;
        }

        // Add slight randomness (±2) and clamp between 0 and 100
        int finalTotal = 0;
        for (int i = 0; i < 6; i++)
        {
            int rand = Random.Range(-2, 3); // -2, -1, 0, 1, 2
            scaled[i] = Mathf.Clamp(scaled[i] + rand, 0, 100);
            finalTotal += scaled[i];
        }

        // Final adjustment to ensure the sum is still targetTotal
        int finalDiff = targetTotal - finalTotal;
        if (finalDiff != 0)
        {
            int maxIdx = 0;
            for (int i = 1; i < 6; i++)
                if (scaled[i] > scaled[maxIdx]) maxIdx = i;
            scaled[maxIdx] = Mathf.Clamp(scaled[maxIdx] + finalDiff, 0, 100);
        }

        // Assign back
        draw = scaled[0];
        takeOut = scaled[1];
        guard = scaled[2];
        sweepStrength = scaled[3];
        sweepEnduro = scaled[4];
        sweepCohesion = scaled[5];
    }
}
