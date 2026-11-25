using NUnit.Framework;

public class AimFuzzBandsTests
{
    [TearDown]
    public void Restore() { AimFuzz.Enabled = true; }

    [Test]
    public void BandsAreTheLargestErrorTheRollCanGive()
    {
        for (int skill = 0; skill <= 100; skill += 25)
        {
            Assert.AreEqual(AimFuzz.AimSigma(skill) * AimFuzz.MaxSigmas, AimFuzzBands.AimHalfWidth(skill), 1e-5f);
            Assert.AreEqual(AimFuzz.WeightSigma(skill) * AimFuzz.MaxSigmas, AimFuzzBands.WeightHalfHeight(skill), 1e-5f);
        }
    }

    [Test]
    public void BetterSkillMeansATighterBand()
    {
        Assert.Greater(AimFuzzBands.WeightHalfHeight(20f), AimFuzzBands.WeightHalfHeight(60f));
        Assert.Greater(AimFuzzBands.AimHalfWidth(20f), AimFuzzBands.AimHalfWidth(60f));
        Assert.AreEqual(0f, AimFuzzBands.WeightHalfHeight(100f));
    }

    [Test]
    public void NoFuzzNoBands()
    {
        AimFuzz.Enabled = false;
        Assert.AreEqual(0f, AimFuzzBands.AimHalfWidth(0f));
        Assert.AreEqual(0f, AimFuzzBands.WeightHalfHeight(0f));
    }
}
