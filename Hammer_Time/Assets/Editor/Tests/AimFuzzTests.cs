using NUnit.Framework;
using UnityEngine;

/// <summary>The skill fuzz on the player's aim line: exact at full skill, bounded by skill, and draws a real throw that ends the full error away.</summary>
public class AimFuzzTests
{
    static readonly Vector2 Launcher = new Vector2(0f, -25f);

    [Test]
    public void FullSkillIsExact()
    {
        Assert.AreEqual(0f, AimFuzz.AimSigma(100f));
        Assert.AreEqual(0f, AimFuzz.WeightSigma(100f));
        var rng = new System.Random(1);
        for (int i = 0; i < 50; i++) Assert.AreEqual(Vector2.zero, AimFuzz.Roll(100f, 100f, rng));
    }

    [Test]
    public void ZeroSkillUsesTheAiSpreads()
    {
        Assert.AreEqual(0.5f, AimFuzz.AimSigma(0f), 1e-6f);
        Assert.AreEqual(0.9f, AimFuzz.WeightSigma(0f), 1e-6f);
        Assert.Less(AimFuzz.AimSigma(60f), AimFuzz.AimSigma(40f));
        Assert.Less(AimFuzz.WeightSigma(60f), AimFuzz.WeightSigma(40f));
    }

    [Test]
    public void RollsMatchTheirSigmaAndNeverPassTheCutoff()
    {
        var rng = new System.Random(7);
        float aimSkill = 30f, weightSkill = 50f;
        float aimSigma = AimFuzz.AimSigma(aimSkill), weightSigma = AimFuzz.WeightSigma(weightSkill);
        int n = 4000;
        double sumX = 0, sumY = 0, sqX = 0, sqY = 0;
        for (int i = 0; i < n; i++)
        {
            Vector2 e = AimFuzz.Roll(aimSkill, weightSkill, rng);
            Assert.LessOrEqual(Mathf.Abs(e.x), aimSigma * AimFuzz.MaxSigmas + 1e-5f);
            Assert.LessOrEqual(Mathf.Abs(e.y), weightSigma * AimFuzz.MaxSigmas + 1e-5f);
            sumX += e.x; sumY += e.y; sqX += e.x * e.x; sqY += e.y * e.y;
        }
        Assert.AreEqual(0.0, sumX / n, aimSigma * 0.1, "sideways error is not biased");
        Assert.AreEqual(0.0, sumY / n, weightSigma * 0.1, "long/short error is not biased");
        // a Gaussian clamped at 2 sigma has a spread of about 0.96 sigma
        Assert.AreEqual(aimSigma * 0.96, System.Math.Sqrt(sqX / n), aimSigma * 0.05);
        Assert.AreEqual(weightSigma * 0.96, System.Math.Sqrt(sqY / n), weightSigma * 0.05);
    }

    static RockPhysicsProfile[] Profiles()
    {
        return new[] { RockPhysicsProfile.Legacy(), RockPhysicsProfile.Arcade(), RockPhysicsProfile.Pro(), RockPhysicsProfile.Real() };
    }

    [Test]
    public void TheBentThrowEndsTheFullErrorAway()
    {
        foreach (var p in Profiles())
            foreach (bool inTurn in new[] { true, false })
                foreach (var error in new[] { new Vector2(-0.5f, 1.8f), new Vector2(0.4f, -1.2f), new Vector2(0.1f, 0f) })
                {
                    Vector2 v = p.LaunchVelocityFromWeightVelocity(new Vector2(0.05f, 8.5f));
                    Vector2 truth = AimFuzz.StopPoint(Launcher, v, inTurn, p);
                    Vector2 bent = AimFuzz.BendThrow(Launcher, v, inTurn, error, p);
                    Vector2 shown = AimFuzz.StopPoint(Launcher, bent, inTurn, p);
                    Assert.AreEqual(truth.x + error.x, shown.x, 0.03f);
                    Assert.AreEqual(truth.y + error.y, shown.y, 0.03f);
                }
    }

    [Test]
    public void NoErrorDrawsTheTrueThrow()
    {
        var p = RockPhysicsProfile.Arcade();
        Vector2 v = p.LaunchVelocityFromWeightVelocity(new Vector2(0f, 8.5f));
        Assert.AreEqual(v, AimFuzz.BendThrow(Launcher, v, true, Vector2.zero, p));
    }

    [Test]
    public void HitArrowsGrowWithTravelAndSkipARockThatStaysPut()
    {
        var p = RockPhysicsProfile.Arcade();
        float Arrow(float travel) { return HitArrow.Length(new Vector2(0f, p.SpeedForStopDistance(travel)), p); }
        Assert.AreEqual(0f, HitArrow.Length(Vector2.zero, p));
        Assert.AreEqual(0f, Arrow(0.2f));
        Assert.Greater(Arrow(1f), 0f);
        Assert.Greater(Arrow(3f), Arrow(1f));
        Assert.AreEqual(HitArrow.MaxLength, Arrow(20f), 1e-4f);
    }
}
