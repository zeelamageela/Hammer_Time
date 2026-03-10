using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CareerStats
{
    public int weightAccuracy;   // Y-axis accuracy (distance/weight control)
    public int aimAccuracy;       // X-axis accuracy (lateral positioning)
    public int finesseAccuracy;   // Complex shot bonus (finesse techniques)
    public int sweepStrength;
    public int sweepEndurance;
    public int sweepCohesion;
}
