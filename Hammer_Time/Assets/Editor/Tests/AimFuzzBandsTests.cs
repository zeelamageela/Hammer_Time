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
    public void OvalIsTheFullErrorRangeButNeverSmallerThanARock()
    {
        var big = AimFuzzBands.OvalSizeFor(0.3f, 1.0f);
        Assert.AreEqual(0.6f, big.x, 1e-5f);
        Assert.AreEqual(2.0f, big.y, 1e-5f);
        var none = AimFuzzBands.OvalSizeFor(0f, 0f);
        Assert.AreEqual(AimFuzzBands.OvalMinSize, none.x);
        Assert.AreEqual(AimFuzzBands.OvalMinSize, none.y);
    }

    [Test]
    public void ConeIsBrightestInTheMiddleAndBlursOutToNothing()
    {
        Assert.AreEqual(1f, AimFuzzBands.ConeProfile(0f, 0.5f));
        Assert.Greater(AimFuzzBands.ConeProfile(0.3f, 0.5f), AimFuzzBands.ConeProfile(0.7f, 0.5f));
        Assert.AreEqual(0f, AimFuzzBands.ConeProfile(1f, 0.5f), 1e-5f);
        Assert.AreEqual(AimFuzzBands.ConeProfile(0.5f, 0f), AimFuzzBands.ConeProfile(0.5f, 1f), "the same all along its length");
    }

    [Test]
    public void NoFuzzNoBands()
    {
        AimFuzz.Enabled = false;
        Assert.AreEqual(0f, AimFuzzBands.AimHalfWidth(0f));
        Assert.AreEqual(0f, AimFuzzBands.WeightHalfHeight(0f));
    }
}
