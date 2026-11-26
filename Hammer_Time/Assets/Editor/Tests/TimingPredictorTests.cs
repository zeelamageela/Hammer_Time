using NUnit.Framework;
using UnityEngine;

/// <summary>The expected-time ranges in the timing panel.</summary>
public class TimingPredictorTests
{
    static RockPhysicsProfile[] Profiles()
    {
        return new[] { RockPhysicsProfile.Legacy(), RockPhysicsProfile.Arcade(), RockPhysicsProfile.Pro(), RockPhysicsProfile.Real() };
    }

    static Vector2 Draw(RockPhysicsProfile p) { return p.LaunchVelocityFromWeightVelocity(new Vector2(0f, 8.5f)); }

    [Test]
    public void SpeedForStopDistanceUndoesStopDistance()
    {
        foreach (var p in Profiles())
        {
            float v = Draw(p).y;
            Assert.AreEqual(v, p.SpeedForStopDistance(p.StopDistance(v)), v * 0.001f);
        }
    }

    [Test]
    public void AnExactShooterGetsOneTime()
    {
        foreach (var p in Profiles())
        {
            TimingPredictor.Predict(Draw(p), true, 0f, 0f, p, out var split, out var hog);
            Assert.AreEqual(split.fast, split.slow, 1e-4f);
            Assert.AreEqual(hog.fast, hog.slow, 1e-3f);
            Assert.Greater(hog.fast, 0f);
        }
    }

    [Test]
    public void TheTrueTimeIsAlwaysInsideTheRange()
    {
        const float half = 1.8f;
        foreach (var p in Profiles())
        {
            Vector2 v = Draw(p);
            float trueSplit = TimingPredictor.Split(v), trueHog = TimingPredictor.HogToHog(v, true, p);
            foreach (float error in new[] { -half, -0.4f, 0f, 0.9f, half })
            {
                TimingPredictor.Predict(v, true, error, half, p, out var split, out var hog);
                Assert.That(trueSplit, Is.InRange(split.fast - 1e-3f, split.slow + 1e-3f));
                Assert.That(trueHog, Is.GreaterThanOrEqualTo(hog.fast - 1e-3f));
                if (hog.slow >= 0f) Assert.That(trueHog, Is.LessThanOrEqualTo(hog.slow + 1e-3f));
            }
        }
    }

    [Test]
    public void ARockThatStopsShortHasNoHogToHog()
    {
        var p = RockPhysicsProfile.Arcade();
        Vector2 weak = new Vector2(0f, p.SpeedForStopDistance(5f));
        Assert.AreEqual(-1f, TimingPredictor.HogToHog(weak, false, p));
    }

    [Test]
    public void RangesReadWell()
    {
        Assert.AreEqual("03:42-03:60", TimingPredictor.Format(new TimingPredictor.Range { fast = 3.42f, slow = 3.6f }));
        Assert.AreEqual("03:42", TimingPredictor.Format(new TimingPredictor.Range { fast = 3.42f, slow = 3.42f }));
        Assert.AreEqual("14:10+", TimingPredictor.Format(new TimingPredictor.Range { fast = 14.1f, slow = -1f }));
        Assert.AreEqual("--", TimingPredictor.Format(new TimingPredictor.Range { fast = -1f, slow = -1f }));
    }
}
