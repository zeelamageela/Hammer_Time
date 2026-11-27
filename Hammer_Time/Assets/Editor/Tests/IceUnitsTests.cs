using NUnit.Framework;
using UnityEngine;

/// <summary>Ice in seconds and feet, and the grading of a read.</summary>
public class IceUnitsTests
{
    static readonly Vector2 Hack = new Vector2(0f, -25f);

    [Test]
    public void SecondsAndFeetGoBothWays()
    {
        Assert.AreEqual(14.5f, IceUnits.Seconds(1f), 1e-4f);
        Assert.AreEqual(3.6f, IceUnits.Feet(1f), 1e-4f);
        Assert.AreEqual(1.15f, IceUnits.SpeedFromSeconds(IceUnits.Seconds(1.15f)), 1e-4f);
        Assert.AreEqual(1.35f, IceUnits.CurlFromFeet(IceUnits.Feet(1.35f)), 1e-4f);
    }

    [Test]
    public void TheFeetAreWhatAButtonDrawReallyBends()
    {
        foreach (var pace in new[] { RockPhysicsProfile.Arcade(), RockPhysicsProfile.Pro(), RockPhysicsProfile.Real() })
        {
            foreach (var ice in new[] { new Vector2(0.85f, 0.65f), new Vector2(1f, 1f), new Vector2(1.15f, 1.35f) })
            {
                var p = pace.WithIce(ice.x, ice.y);
                float button = pace.StopDistance(pace.LaunchSpeedFromWeight(8.5f));   // the same spot on every sheet
                Vector2 draw = p.LaunchVelocityFromWeightVelocity(new Vector2(0f, p.WeightFromLaunchSpeed(p.SpeedForStopDistance(button))));
                float feet = Mathf.Abs(AimFuzz.StopPoint(Hack, draw, true, p).x) * IceUnits.FeetPerUnit;
                Assert.AreEqual(IceUnits.Feet(ice.y), feet, 0.35f);
            }
        }
    }

    [Test]
    public void EveryEventsIceFitsOnTheSliders()
    {
        for (int bg = 0; bg < 6; bg++)
            for (int venue = 0; venue < 500; venue++)
            {
                VenueIce.Sheet(bg, 5, venue, venue % 9, out float speed, out float curl);
                Assert.That(IceUnits.Seconds(speed), Is.InRange(IceUnits.MinSeconds, IceUnits.MaxSeconds));
                Assert.That(IceUnits.Feet(curl), Is.InRange(IceUnits.MinFeet, IceUnits.MaxFeet));
            }
    }

    static void Same(Color wanted, Color got)
    {
        Assert.Less(Vector4.Distance(wanted, got), 0.001f);
    }

    [Test]
    public void TheGradeFollowsHowWrongTheReadIs()
    {
        var real = RockPhysicsProfile.Arcade().WithIce(1.15f, 1.35f);
        Vector2 draw = new Vector2(0f, 7.4f);
        float exact = ReadGrade.MissFeet(Hack, draw, true, real, real);
        float close = ReadGrade.MissFeet(Hack, draw, true, RockPhysicsProfile.Arcade().WithIce(1.12f, 1.35f), real);
        float off = ReadGrade.MissFeet(Hack, draw, true, RockPhysicsProfile.Arcade(), real);
        Assert.AreEqual(0f, exact, 1e-3f);
        Assert.Greater(close, 0.5f);
        Assert.Greater(off, close);
        Same(ReadGrade.Green, ReadGrade.Colour(exact));
        Same(ReadGrade.Red, ReadGrade.Colour(ReadGrade.OffFeet + 1f));
        Same(ReadGrade.Yellow, ReadGrade.Colour(ReadGrade.CloseFeet));
    }
}
