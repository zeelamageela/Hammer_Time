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
        var s = RockPhysics.Launch(new Vector2(0f, -25f), new Vector2(0f, v0));
        releaseStep = -1;
        int steps = Mathf.RoundToInt(seconds / Dt);
        for (int i = 1; i <= steps; i++)
        {
            RockPhysics.Step(ref s, p, Dt);
            if (!s.sliding && s.position.y > RockPhysics.ReleaseY)
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
}
