using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// The constant-deceleration law (Arcade) must give the same PATHS and the same weight-to-distance map as the old
/// exponential rock (Legacy), and Real() must be exactly Arcade slowed down in time.
/// </summary>
public class ConstantDecelTests
{
    const float Dt = 0.02f;
    static readonly float[] Weights = { 5f, 5.5f, 6f, 6.5f, 7f, 8f, 8.5f, 9f, 9.35f, 10f, 11f, 12.4f, 13.75f, 15.8f };

    struct Run
    {
        public List<Vector2> points;   // every step
        public List<float> times;
        public float releaseTime;
        public float hogTime;          // crossing of y = -16.15 (near hog)
        public float farHogTime;       // crossing of y = 0
        public float stopTime;
        public Vector2 end;
    }

    static Run Fly(RockPhysicsProfile p, float weight, bool inTurn, float startY = -25f)
    {
        var s = RockPhysics.Launch(new Vector2(0f, startY), new Vector2(0f, p.LaunchSpeedFromWeight(weight)), p);
        var r = new Run { points = new List<Vector2>(), times = new List<float>(), releaseTime = -1f, hogTime = -1f, farHogTime = -1f };
        float t = 0f;
        Vector2 prev = s.position;
        for (int i = 0; i < 20000 && !s.asleep; i++)
        {
            RockPhysics.Step(ref s, p, Dt);
            t += Dt;
            if (s.freeFlight && s.position.y > RockPhysics.ReleaseY) { RockPhysics.Release(ref s, p, inTurn); r.releaseTime = t; }
            if (r.hogTime < 0 && prev.y < -16.15f && s.position.y >= -16.15f) r.hogTime = t - Dt + Dt * (-16.15f - prev.y) / (s.position.y - prev.y);
            if (r.farHogTime < 0 && prev.y < 0f && s.position.y >= 0f) r.farHogTime = t - Dt + Dt * (0f - prev.y) / (s.position.y - prev.y);
            r.points.Add(s.position); r.times.Add(t);
            prev = s.position;
        }
        r.stopTime = t;
        r.end = s.position;
        return r;
    }

    static float XAt(Run r, float y)
    {
        for (int i = 1; i < r.points.Count; i++)
            if (r.points[i - 1].y <= y && r.points[i].y >= y)
                return Mathf.Lerp(r.points[i - 1].x, r.points[i].x, (y - r.points[i - 1].y) / Mathf.Max(1e-6f, r.points[i].y - r.points[i - 1].y));
        return r.points[r.points.Count - 1].x;
    }

    [Test]
    public void Arcade_FollowsTheSamePathsAsTheOldRock_AtEveryWeight()
    {
        var legacy = RockPhysicsProfile.Legacy();
        var arcade = RockPhysicsProfile.Arcade();
        foreach (float w in Weights)
        foreach (bool inTurn in new[] { false, true })
        {
            var a = Fly(legacy, w, inTurn);
            var b = Fly(arcade, w, inTurn);
            float top = Mathf.Min(a.end.y, b.end.y) - 0.1f;
            float worst = 0f;
            for (float y = RockPhysics.ReleaseY + 0.1f; y < top; y += 0.25f)
                worst = Mathf.Max(worst, Mathf.Abs(XAt(a, y) - XAt(b, y)));
            Assert.Less(worst, 0.06f, $"sideways path differs by {worst:F3} at weight {w} ({(inTurn ? "in" : "out")}-turn)");
            Assert.AreEqual(a.end.y, b.end.y, 0.25f, $"stopping distance at weight {w} (the legacy rock's own end point wobbles by up to ~0.2 with its sleep timing)");
        }
    }

    [Test]
    public void Arcade_KeepsTheOldHogToHogPace_ButStopsMuchSooner()
    {
        var a = Fly(RockPhysicsProfile.Legacy(), 8.5f, false);
        var b = Fly(RockPhysicsProfile.Arcade(), 8.5f, false);
        float oldHH = a.farHogTime - a.hogTime;
        float newHH = b.farHogTime - b.hogTime;
        Assert.AreEqual(3.28f, oldHH, 0.1f, "legacy button draw hog-to-hog (Phase 0 measurement)");
        Assert.AreEqual(oldHH, newHH, 0.25f, "Arcade should keep the game's pace between the hog lines");
        Assert.Less(b.stopTime, a.stopTime * 0.6f, "dry friction ends the slide instead of creeping");
    }

    [Test]
    public void Real_IsArcadeSlowedDown_ExactlySamePath()
    {
        var arcade = RockPhysicsProfile.Arcade();
        var real = RockPhysicsProfile.Real();
        foreach (float w in new[] { 6f, 8.5f, 9.35f, 12.4f, 15.8f })
        foreach (bool inTurn in new[] { false, true })
        {
            var a = Fly(arcade, w, inTurn);
            var b = Fly(real, w, inTurn);
            Assert.AreEqual(a.end.x, b.end.x, 0.02f, $"end x at weight {w}");
            Assert.AreEqual(a.end.y, b.end.y, 0.1f, $"end y at weight {w}");
            float top = a.end.y - 0.1f;
            float worst = 0f;
            for (float y = RockPhysics.ReleaseY + 0.1f; y < top; y += 0.25f)
                worst = Mathf.Max(worst, Mathf.Abs(XAt(a, y) - XAt(b, y)));
            Assert.Less(worst, 0.02f, $"path differs by {worst:F3} at weight {w}");
            Assert.AreEqual(RockPhysicsProfile.RealTimeStretch, b.stopTime / a.stopTime, 0.3f, $"slow-down factor at weight {w} (a fixed 0.5 s sleep timer is not stretched)");
        }
    }

    [Test]
    public void Real_ButtonDraw_TakesAboutFourteenAndAHalfSecondsHogToHog()
    {
        var r = Fly(RockPhysicsProfile.Real(), 8.5f, false);
        Assert.AreEqual(14.5f, r.farHogTime - r.hogTime, 0.6f);
    }

    [Test]
    public void WeightAndSpeedConvertBothWays_AndSpeedPredictsTheStoppingDistance()
    {
        foreach (var p in new[] { RockPhysicsProfile.Arcade(), RockPhysicsProfile.Real() })
        foreach (float w in Weights)
        {
            float v = p.LaunchSpeedFromWeight(w);
            Assert.AreEqual(w, p.WeightFromLaunchSpeed(v), 0.001f);
            var run = Fly(p, w, false);
            // the rock travels StopDistance(v) after the near hog line (release happens ~0.01 before it)
            Assert.AreEqual(RockPhysics.ReleaseY + p.StopDistance(v), run.end.y, 0.15f, $"weight {w}");
        }
        var legacy = RockPhysicsProfile.Legacy();
        Assert.AreEqual(8.5f, legacy.LaunchSpeedFromWeight(8.5f));
    }

    [Test]
    public void ARockThatWasSittingStill_SlidesAShortWay_AndStops()
    {
        var p = RockPhysicsProfile.Arcade();
        var s = RockPhysics.NewRestingRock(p);
        s.velocity = new Vector2(0f, 4f);
        float t = 0f;
        while (!s.asleep && t < 60f) { RockPhysics.Step(ref s, p, Dt); t += Dt; }
        Assert.IsTrue(s.asleep);
        Assert.Greater(s.position.y, 3f);
        Assert.Less(s.position.y, 4.6f, "old hit-rock behaviour: about speed / 0.9 = 4.4");
        Assert.Less(t, 12f);
    }

    [Test]
    public void SweepingScale_LowersFriction_SoTheRockGoesFarther()
    {
        var p = RockPhysicsProfile.Arcade();
        var plain = RockPhysics.Launch(new Vector2(0f, RockPhysics.ReleaseY), new Vector2(0f, p.LaunchSpeedFromWeight(8f)), p);
        var swept = plain;
        RockPhysics.Release(ref plain, p, false);
        RockPhysics.Release(ref swept, p, false);
        swept.frictionScale = 0.8f;
        for (int i = 0; i < 4000 && !plain.asleep; i++) RockPhysics.Step(ref plain, p, Dt);
        for (int i = 0; i < 4000 && !swept.asleep; i++) RockPhysics.Step(ref swept, p, Dt);
        Assert.Greater(swept.position.y, plain.position.y + 2f);
    }

    [Test]
    public void TrajectorySimulator_StopsAtTheSamePlace_UnderEveryProfile()
    {
        var sim = new TrajectorySimulator(0f, 0f);
        try
        {
            foreach (float w in new[] { 6f, 8.5f, 9.35f, 12.4f, 15.8f })
            foreach (bool inTurn in new[] { false, true })
            {
                RockPhysicsSettings.Use(RockPhysicsProfile.Legacy());
                var legacy = sim.SimulateTrajectory(new Vector2(0f, -25f), new Vector2(0f, w), inTurn, 400, null, false);
                RockPhysicsSettings.Use(RockPhysicsProfile.Arcade());
                var arcade = sim.SimulateTrajectory(new Vector2(0f, -25f), new Vector2(0f, w), inTurn, 400, null, false);
                int arcadePoints = arcade.Count;
                RockPhysicsSettings.Use(RockPhysicsProfile.Real());
                var real = sim.SimulateTrajectory(new Vector2(0f, -25f), new Vector2(0f, w), inTurn, 400, null, false);

                Vector2 a = legacy[legacy.Count - 1], b = arcade[arcade.Count - 1], c = real[real.Count - 1];
                Assert.AreEqual(a.x, b.x, 0.1f, $"arcade end x, weight {w}");
                Assert.AreEqual(a.y, b.y, 0.25f, $"arcade end y, weight {w}");
                Assert.AreEqual(b.x, c.x, 0.03f, $"real end x, weight {w}");
                Assert.AreEqual(b.y, c.y, 0.1f, $"real end y, weight {w} (the simulator's time cap must stretch with the profile)");
                Assert.Less(real.Count, 400, "point spacing stretches with the profile");
                Assert.AreEqual(arcadePoints, real.Count, 25, "same number of points at any pace");
            }
        }
        finally { RockPhysicsSettings.Use(RockPhysicsProfile.Legacy()); }
    }

    [Test]
    public void SimulatingFromARocksRealState_AgreesWithSimulatingTheWholeThrow()
    {
        var sim = new TrajectorySimulator(0f, 0f);
        try
        {
            foreach (var profile in new[] { RockPhysicsProfile.Arcade(), RockPhysicsProfile.Real() })
            {
                RockPhysicsSettings.Use(profile);
                float w = 9.35f;
                var whole = sim.SimulateTrajectory(new Vector2(0f, -25f), new Vector2(0f, w), true, 400, null, false);

                // fly the same rock by hand for a while, then hand its state over
                var s = RockPhysics.Launch(new Vector2(0f, -25f), new Vector2(0f, profile.LaunchSpeedFromWeight(w)), profile);
                for (int i = 0; i < Mathf.RoundToInt(5f * profile.timeStretch / Dt); i++)
                {
                    RockPhysics.Step(ref s, profile, Dt);
                    if (s.freeFlight && s.position.y > RockPhysics.ReleaseY) RockPhysics.Release(ref s, profile, true);
                }
                Assert.IsFalse(s.freeFlight, "should be past the hog line");
                var fromState = sim.SimulateTrajectoryFromState(s, true, 400, null);

                Assert.AreEqual(whole[whole.Count - 1].x, fromState[fromState.Count - 1].x, 0.03f);
                Assert.AreEqual(whole[whole.Count - 1].y, fromState[fromState.Count - 1].y, 0.1f);
            }
        }
        finally { RockPhysicsSettings.Use(RockPhysicsProfile.Legacy()); }
    }
}
