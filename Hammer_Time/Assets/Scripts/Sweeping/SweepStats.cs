using UnityEngine;

/// <summary>
/// What cohesion (sweeping in sync) adds: it scales the two-broom sweeps (weight and hard) by the two sweepers' average. Neutral at
/// 50 (an average pair sweeps as before), up to Swing either way.
/// </summary>
public static class SweepStats
{
    public const float Swing = 0.15f;

    public static float CohesionFactor(float leftCohesion, float rightCohesion)
    {
        float average = Mathf.Clamp((leftCohesion + rightCohesion) * 0.5f, 0f, 100f);
        return 1f + Swing * (average - 50f) / 50f;
    }
}
