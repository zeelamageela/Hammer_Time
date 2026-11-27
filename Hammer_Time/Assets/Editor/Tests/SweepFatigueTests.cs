using NUnit.Framework;

public class SweepFatigueTests
{
    [Test]
    public void WeakestSweeperLastsHalfARunAndStrongestAWholeOne()
    {
        Assert.AreEqual(5f, SweepFatigue.SweepSeconds(0f, false, 10f), 1e-3f);
        Assert.AreEqual(10f, SweepFatigue.SweepSeconds(100f, false, 10f), 1e-3f);
        Assert.AreEqual(2.5f, SweepFatigue.SweepSeconds(0f, true, 10f), 1e-3f);
    }

    [Test]
    public void MoreEnduranceLastsLongerAndRecoversSooner()
    {
        Assert.Greater(SweepFatigue.SweepSeconds(80f, false, 10f), SweepFatigue.SweepSeconds(20f, false, 10f));
        Assert.Less(SweepFatigue.RestSeconds(80f, 10f), SweepFatigue.RestSeconds(20f, 10f));
    }

    [Test]
    public void SlowerPacesGiveLongerRuns()
    {
        float arcade = SweepFatigue.RunSeconds(RockPhysicsProfile.Arcade());
        float pro = SweepFatigue.RunSeconds(RockPhysicsProfile.Pro());
        float real = SweepFatigue.RunSeconds(RockPhysicsProfile.Real());
        Assert.That(arcade, Is.InRange(9f, 12f));
        Assert.AreEqual(arcade * RockPhysicsProfile.ProTimeStretch, pro, 0.1f);
        Assert.That(real, Is.InRange(28f, 34f));
    }
}
