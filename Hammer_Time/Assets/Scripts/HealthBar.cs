using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    public Transform bar;

    public CharacterStats stats;

    float healthScale;

    // Update is called once per frame
    void Update()
    {
        healthScale = stats.sweepHealth / 100f;
        bar.localScale = new Vector3(healthScale, 1, 1);
    }
}
