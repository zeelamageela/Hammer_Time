using NUnit.Framework;
using UnityEngine;

/// <summary>The skill fuzz on the player's aim line: exact at full skill, bounded by skill, and bends the line from nothing at the hack to the full error at the end.</summary>
public class AimFuzzTests
{
    static readonly Vector2 Launcher = new Vector2(0f, -25f);
    static readonly Vector2 End = new Vector2(0.4f, 6.5f);

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
        Assert.AreEqual(0.35f, AimFuzz.AimSigma(0f), 1e-6f);
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

    [Test]
    public void WarpMovesTheEndByTheFullErrorAndLeavesTheHackAlone()
    {
        Vector2 error = new Vector2(-0.12f, 0.6f);
        Vector2 atHack = AimFuzz.Warp(Launcher, Launcher, End, error);
        Assert.AreEqual(Launcher.x, atHack.x, 1e-5f);
        Assert.AreEqual(Launcher.y, atHack.y, 1e-5f);

        Vector2 atEnd = AimFuzz.Warp(End, Launcher, End, error);
        Assert.AreEqual(End.x + error.x, atEnd.x, 1e-4f);
        Assert.AreEqual(End.y + error.y, atEnd.y, 1e-4f);
    }

    [Test]
    public void WarpIsSmoothAndKeepsTheLineInOrder()
    {
        Vector2 error = new Vector2(0.15f, -0.9f);
        float lastY = float.MinValue, lastX = float.MinValue;
        for (int i = 0; i <= 100; i++)
        {
            Vector2 p = Vector2.Lerp(Launcher, End, i / 100f);
            Vector2 w = AimFuzz.Warp(p, Launcher, End, error);
            Assert.Greater(w.y, lastY, "the line must keep going forward");
            Assert.GreaterOrEqual(w.x, lastX - 1e-5f, "sideways error grows steadily along the line");
            lastY = w.y; lastX = w.x;
        }
    }

    [Test]
    public void NoErrorIsTheIdentity()
    {
        Vector2 p = new Vector2(0.2f, -3f);
        Vector2 w = AimFuzz.Warp(p, Launcher, End, Vector2.zero);
        Assert.AreEqual(p.x, w.x, 1e-5f);
        Assert.AreEqual(p.y, w.y, 1e-5f);
    }
}
