using NUnit.Framework;
using UnityEngine;

/// <summary>The timing panel's real-ice seconds.</summary>
public class IceClockTests
{
    static RockPhysicsProfile[] Paces()
    {
        return new[] { RockPhysicsProfile.Arcade(), RockPhysicsProfile.Arcade(0.8f), RockPhysicsProfile.Pro(), RockPhysicsProfile.Real() };
    }

    static void Read(RockPhysicsProfile p, float weight, out float split, out float hogToHog)
    {
        Vector2 v = p.LaunchVelocityFromWeightVelocity(new Vector2(0f, weight));
        split = IceClock.Split(TimingPredictor.Split(v), p);
        hogToHog = IceClock.HogToHog(TimingPredictor.HogToHog(v, true, p), p);
    }

    [Test]
    public void AButtonDrawReadsTheSameOnEveryPace()
    {
        foreach (var p in Paces())
        {
            Read(p, 8.5f, out float split, out float hog);
            Assert.AreEqual(IceClock.ButtonSplit, split, 0.02f);
            Assert.AreEqual(IceClock.ButtonHogToHog, hog, 0.1f);
        }
    }

    [Test]
    public void EveryWeightReadsTheSameOnEveryPace()
    {
        foreach (float weight in new[] { 6.5f, 7.5f, 9.5f, 13f, 16f })
        {
            Read(RockPhysicsProfile.Real(), weight, out float split, out float hog);
            foreach (var p in Paces())
            {
                Read(p, weight, out float s, out float h);
                Assert.AreEqual(split, s, 0.02f);
                Assert.AreEqual(hog, h, 0.2f);
            }
        }
    }

    [Test]
    public void NoTimeStaysNoTime()
    {
        Assert.AreEqual(-1f, IceClock.Split(-1f, RockPhysicsProfile.Pro()));
        Assert.AreEqual(-1f, IceClock.HogToHog(-1f, RockPhysicsProfile.Pro()));
    }
}
