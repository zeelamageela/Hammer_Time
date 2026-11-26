using NUnit.Framework;

/// <summary>Cohesion in sweeping, consistency in aiming, gear points and sponsor rules.</summary>
public class DesignRulesTests
{
    [Test]
    public void AnAverageSweeperSweepsAsBefore()
    {
        Assert.AreEqual(1f, SweepStats.CohesionFactor(50f, 50f), 1e-5f);
        Assert.AreEqual(1f + SweepStats.Swing, SweepStats.CohesionFactor(100f, 100f), 1e-5f);
        Assert.AreEqual(1f - SweepStats.Swing, SweepStats.CohesionFactor(0f, 0f), 1e-5f);
    }

    [Test]
    public void ConsistencySetsTheWorstMissAndTheOval()
    {
        Assert.AreEqual(AimFuzz.MaxSigmas, AimFuzz.MaxSigmasFor(50f), 1e-5f);
        Assert.AreEqual(AimFuzz.MaxSigmas * 0.75f, AimFuzz.MaxSigmasFor(100f), 1e-5f);
        Assert.AreEqual(AimFuzz.MaxSigmas * 1.25f, AimFuzz.MaxSigmasFor(0f), 1e-5f);
        var rng = new System.Random(3);
        foreach (float consistency in new[] { 0f, 50f, 100f })
        {
            float wide = AimFuzzBands.AimHalfWidth(30f, consistency), tall = AimFuzzBands.WeightHalfHeight(30f, consistency);
            for (int i = 0; i < 2000; i++)
            {
                var e = AimFuzz.Roll(30f, 30f, rng, consistency);
                Assert.LessOrEqual(System.Math.Abs(e.x), wide + 1e-5f);
                Assert.LessOrEqual(System.Math.Abs(e.y), tall + 1e-5f);
            }
        }
    }

    [Test]
    public void ATopItemCarriesTwentyPoints()
    {
        Assert.AreEqual(20, EquipmentManager.PointsFor(10000f));
        Assert.AreEqual(20, EquipmentManager.PointsFor(15000f));
        Assert.AreEqual(10, EquipmentManager.PointsFor(10000f, 0.5f));
        Assert.AreEqual(0, EquipmentManager.PointsFor(0f));
    }

    [Test]
    public void EachSponsorTierHasOneRule()
    {
        Assert.IsFalse(SponsorRules.Available(SponsorRules.Tier.Local, SponsorRules.LocalFromWeek - 1, true, 99));
        Assert.IsTrue(SponsorRules.Available(SponsorRules.Tier.Local, SponsorRules.LocalFromWeek, false, 0));
        Assert.IsFalse(SponsorRules.Available(SponsorRules.Tier.National, 30, false, 99));
        Assert.IsTrue(SponsorRules.Available(SponsorRules.Tier.National, 1, true, 0));
        Assert.IsFalse(SponsorRules.Available(SponsorRules.Tier.International, 30, true, SponsorRules.InternationalWins - 1));
        Assert.IsTrue(SponsorRules.Available(SponsorRules.Tier.International, 1, false, SponsorRules.InternationalWins));
    }

    [Test]
    public void ResultsPayBonusesAndContractsHaveALength()
    {
        Assert.AreEqual(750f, SponsorRules.ResultBonus(250f, 1));
        Assert.AreEqual(250f, SponsorRules.ResultBonus(250f, 4));
        Assert.AreEqual(0f, SponsorRules.ResultBonus(250f, 5));
        Assert.AreEqual(0f, SponsorRules.ResultBonus(250f, -1));
        Assert.AreEqual(SponsorRules.DefaultTerm, SponsorRules.Term(0));
        Assert.AreEqual(7, SponsorRules.Term(7));
    }
}
