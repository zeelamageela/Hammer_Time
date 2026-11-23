using NUnit.Framework;
using UnityEngine;

/// <summary>
/// RockPhysics must reproduce the live rock exactly. The expected numbers were recorded from the real rock (Rigidbody2D,
/// Rock_Force, hog-line trigger) in TournyGame by AgentScripts/Measure.cs Golden(), stepping 0.02 s from the hack at
/// (0, -25) with no sweeping. If one of these fails, the model (or the profile defaults) no longer plays like the game did.
/// </summary>
public class RockPhysicsParityTests
{
    const float Dt = 0.02f;
    const float Tol = 0.005f;

    struct Sample { public float t, x, y; public Sample(float t, float x, float y) { this.t = t; this.x = x; this.y = y; } }

    static RockState Fly(float v0, bool inTurn, RockPhysicsProfile p, out int releaseStep, float seconds, System.Action<float, RockState> onStep = null)
    {
        var s = RockPhysics.Launch(new Vector2(0f, -25f), new Vector2(0f, v0), p);
        releaseStep = -1;
        int steps = Mathf.RoundToInt(seconds / Dt);
        for (int i = 1; i <= steps; i++)
        {
            RockPhysics.Step(ref s, p, Dt);
            if (s.freeFlight && s.position.y > RockPhysics.ReleaseY)
            {
                RockPhysics.Release(ref s, p, inTurn);
                releaseStep = i;
            }
            if (onStep != null) onStep(i * Dt, s);
        }
        return s;
    }

    static void AssertPath(float v0, bool inTurn, Sample[] expected, Sample final, int expectedReleaseStep)
    {
        var p = RockPhysicsProfile.Arcade();
        var got = new System.Collections.Generic.Dictionary<int, RockState>();
        int release;
        var end = Fly(v0, inTurn, p, out release, 30f, (t, st) => got[Mathf.RoundToInt(t / Dt)] = st);

        Assert.AreEqual(expectedReleaseStep, release, "release step (when the rock leaves the near hog trigger)");
        float sign = inTurn ? -1f : 1f;   // recorded out-turn; in-turn is the mirror image
        foreach (var e in expected)
        {
            var st = got[Mathf.RoundToInt(e.t / Dt)];
            Assert.AreEqual(sign * e.x, st.position.x, Tol, "x at t=" + e.t + " v0=" + v0);
            Assert.AreEqual(e.y, st.position.y, Tol, "y at t=" + e.t + " v0=" + v0);
        }
        Assert.AreEqual(sign * final.x, end.position.x, Tol, "final x v0=" + v0);
        Assert.AreEqual(final.y, end.position.y, Tol, "final y v0=" + v0);
        Assert.IsTrue(end.asleep, "rock should have come to rest");
    }

    [Test]
    public void Draw_Light_6()
    {
        AssertPath(6f, false, new[]
        {
            new Sample(2f, -0.0113f, -13.2365f), new Sample(3f, -0.0940f, -9.0556f), new Sample(4f, -0.2156f, -6.1923f),
            new Sample(8f, -0.6731f, -1.3390f), new Sample(12f, -0.9014f, -0.2714f), new Sample(16f, -0.9873f, -0.0366f),
        }, new Sample(0f, -1.0117f, 0.0084f), 77);
    }

    [Test]
    public void Draw_Button_8_5()
    {
        AssertPath(8.5f, false, new[]
        {
            new Sample(2f, -0.0380f, -9.2415f), new Sample(4f, -0.2507f, -0.8572f), new Sample(8f, -0.6459f, 4.9195f),
            new Sample(12f, -0.8332f, 6.1903f), new Sample(16f, -0.9024f, 6.4698f),
        }, new Sample(0f, -0.9234f, 6.5268f), 54);
    }

    [Test]
    public void Takeout_12_4()
    {
        AssertPath(12.4f, false, new[]
        {
            new Sample(1f, -0.0014f, -12.7652f), new Sample(2f, -0.0284f, -3.4452f), new Sample(4f, -0.1247f, 7.3089f),
            new Sample(8f, -0.2866f, 14.7183f), new Sample(12f, -0.3607f, 16.3482f),
        }, new Sample(0f, -0.3967f, 16.7856f), 37);
    }

    [Test]
    public void InTurn_IsTheMirrorOfOutTurn()
    {
        AssertPath(8.5f, true, new[]
        {
            new Sample(2f, -0.0380f, -9.2415f), new Sample(8f, -0.6459f, 4.9195f), new Sample(16f, -0.9024f, 6.4698f),
        }, new Sample(0f, -0.9234f, 6.5268f), 54);
    }

    [Test]
    public void SpinAndSpeedMatchTheRecording()
    {
        var p = RockPhysicsProfile.Arcade();
        int release;
        RockState at2 = default;
        Fly(8.5f, false, p, out release, 2f, (t, st) => { if (Mathf.Approximately(Mathf.Round(t * 50f), 100f)) at2 = st; });
        Assert.AreEqual(254.984f, at2.spin, 0.1f, "spin at t=2.00");
        Assert.AreEqual(6.00019f, at2.velocity.magnitude, 0.01f, "speed at t=2.00");
    }

    [Test]
    public void ConstantDecel_StopsALightRockSoonerAndAHeavyOneLater_ThanZero()
    {
        // Sanity for the new dry-friction term: with it on, a rock still stops, and stops sooner than without it.
        var off = RockPhysicsProfile.Arcade();
        var on = RockPhysicsProfile.Arcade(); on.constantDecel = 0.3f;
        int r;
        var a = Fly(8.5f, false, off, out r, 30f);
        var b = Fly(8.5f, false, on, out r, 30f);
        Assert.Less(b.position.y, a.position.y);
        Assert.IsTrue(b.asleep);
    }

    // Rest points of the live rock (AgentScripts/Measure.cs, out-turn, from the hack). The aim preview and the AI use
    // TrajectorySimulator, so it has to stop where the real rock stops.
    static readonly float[][] LiveRest =
    {
        new[] { 5.0f, -1.04f, -2.67f }, new[] { 6.0f, -1.01f, 0.01f }, new[] { 8.0f, -0.94f, 5.31f }, new[] { 8.5f, -0.92f, 6.52f },
        new[] { 9.35f, -0.92f, 8.74f }, new[] { 10.0f, -0.72f, 10.49f }, new[] { 12.4f, -0.40f, 16.78f }, new[] { 15.8f, -0.24f, 25.72f },
    };

    [Test]
    public void TrajectorySimulator_StopsWhereTheLiveRockStops()
    {
        var sim = new TrajectorySimulator(0.62f, 0.25f);   // constructor arguments are ignored now
        foreach (var row in LiveRest)
        {
            var path = sim.SimulateTrajectory(new Vector2(0f, -25f), new Vector2(0f, row[0]), false, 600, null, false);
            var end = path[path.Count - 1];
            Assert.AreEqual(row[1], end.x, 0.03f, "x at v0=" + row[0]);
            Assert.AreEqual(row[2], end.y, 0.03f, "y at v0=" + row[0]);
        }
    }

    [Test]
    public void TrajectorySimulator_InTurnMirrorsOutTurn()
    {
        var sim = new TrajectorySimulator(0.62f, 0.25f);
        var outTurn = sim.SimulateTrajectory(new Vector2(0f, -25f), new Vector2(0f, 8.5f), false, 600, null, false);
        var inTurn = sim.SimulateTrajectory(new Vector2(0f, -25f), new Vector2(0f, 8.5f), true, 600, null, false);
        Assert.AreEqual(-outTurn[outTurn.Count - 1].x, inTurn[inTurn.Count - 1].x, 0.001f);
        Assert.AreEqual(outTurn[outTurn.Count - 1].y, inTurn[inTurn.Count - 1].y, 0.001f);
    }

    [Test]
    public void TrajectorySimulator_FastRockCannotTunnelThroughARock()
    {
        var sim = new TrajectorySimulator(0.62f, 0.25f);
        var target = new GameObject("TunnelTestRock");
        try
        {
            target.transform.position = new Vector3(0f, 6.5f, 0f);
            var path = sim.SimulateTrajectory(new Vector2(0f, -25f), new Vector2(0f, 16f), false, 600, new System.Collections.Generic.List<GameObject> { target }, false);
            Assert.IsTrue(sim.GetCollisionInfo().hasCollision, "a 16 u/s rock moves 0.32 per step (more than a rock is wide) and must still hit");
        }
        finally { Object.DestroyImmediate(target); }
    }
}
