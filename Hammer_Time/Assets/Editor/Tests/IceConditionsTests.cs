using NUnit.Framework;
using UnityEngine;

/// <summary>The same rock on faster and slower ice.</summary>
public class IceConditionsTests
{
    static RockPhysicsProfile[] Paces()
    {
        return new[] { RockPhysicsProfile.Arcade(), RockPhysicsProfile.Pro(), RockPhysicsProfile.Real() };
    }

    // throws a weight up the middle and returns where it stops
    static Vector2 Rest(RockPhysicsProfile p, float weight, bool inTurn)
    {
        RockState rock = RockPhysics.Launch(new Vector2(0f, -25f), p.LaunchVelocityFromWeightVelocity(new Vector2(0f, weight)), p);
        for (int i = 0; i < 20000 && !rock.asleep; i++)
        {
            RockPhysics.Step(ref rock, p, 0.02f);
            if (rock.freeFlight && rock.position.y > RockPhysics.ReleaseY) RockPhysics.Release(ref rock, p, inTurn);
        }
        return rock.position;
    }

    [Test]
    public void TheThrowIsTheSameOnAnyIce()
    {
        foreach (var p in Paces())
        {
            var fast = p.WithIce(1.15f, 1.35f);
            Assert.AreEqual(p.LaunchSpeedFromWeight(8.5f), fast.LaunchSpeedFromWeight(8.5f), 1e-4f);
            Assert.AreEqual(8.5f, fast.WeightFromLaunchSpeed(fast.LaunchSpeedFromWeight(8.5f)), 1e-3f);
        }
    }

    [Test]
    public void FasterIceCarriesTheSameWeightFurther()
    {
        foreach (var p in Paces())
        {
            float normal = Rest(p, 8.5f, true).y - RockPhysics.ReleaseY;
            Assert.AreEqual(normal * 1.15f, Rest(p.WithIce(1.15f, 1f), 8.5f, true).y - RockPhysics.ReleaseY, 0.25f);
            Assert.AreEqual(normal * 0.85f, Rest(p.WithIce(0.85f, 1f), 8.5f, true).y - RockPhysics.ReleaseY, 0.25f);
        }
    }

    [Test]
    public void AShotToTheSameSpotBendsByTheCurlSetting()
    {
        foreach (var p in Paces())
        {
            Vector2 normal = Rest(p, 8.5f, true);
            foreach (float speed in new[] { 0.85f, 1.15f })
            {
                var ice = p.WithIce(speed, 1f);
                // the weight that reaches the same distance on this ice
                float weight = ice.WeightFromLaunchSpeed(ice.SpeedForStopDistance(p.StopDistance(p.LaunchSpeedFromWeight(8.5f))));
                Vector2 same = Rest(ice, weight, true);
                Assert.AreEqual(normal.y, same.y, 0.15f);
                Assert.AreEqual(normal.x, same.x, 0.05f);

                var curly = p.WithIce(speed, 1.35f);
                Assert.AreEqual(normal.x * 1.35f, Rest(curly, weight, true).x, 0.12f);
            }
        }
    }

    [Test]
    public void TheClocksReadLongerOnFasterIce()
    {
        var p = RockPhysicsProfile.Pro();
        foreach (float speed in new[] { 0.85f, 1.15f })
        {
            var ice = p.WithIce(speed, 1f);
            Vector2 draw = Vector2.up * ice.SpeedForStopDistance(p.StopDistance(p.LaunchSpeedFromWeight(8.5f)));
            Assert.AreEqual(IceClock.ButtonSplit * Mathf.Sqrt(speed), IceClock.Split(TimingPredictor.Split(draw), ice), 0.03f);
            Assert.AreEqual(IceClock.ButtonHogToHog * Mathf.Sqrt(speed), IceClock.HogToHog(TimingPredictor.HogToHog(draw, true, ice), ice), 0.2f);
        }
    }
}
