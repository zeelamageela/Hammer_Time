using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Transform bar;

    public CharacterStats stats;

    float healthScale;

    public Color green;
    public Color red;

    // Update is called once per frame
    void Update()
    {
        healthScale = stats.sweepHealth / 100f;
        bar.localScale = new Vector3(healthScale, 1, 1);


    }
}
