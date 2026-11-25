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
    public void BandIsSolidInTheMiddleAndSoftAtTheEdge()
    {
        Assert.AreEqual(1f, AimFuzzBands.BandProfile(0f));
        Assert.AreEqual(1f, AimFuzzBands.BandProfile(0.5f), "the solid part is wide");
        Assert.AreEqual(0f, AimFuzzBands.BandProfile(1f), 1e-5f);
        Assert.AreEqual(AimFuzzBands.BandProfile(-0.8f), AimFuzzBands.BandProfile(0.8f), "same both sides");
        float last = 1f;
        for (float a = 0.5f; a <= 1f; a += 0.05f)
        {
            float v = AimFuzzBands.BandProfile(a);
            Assert.LessOrEqual(v, last + 1e-5f, "only ever fades outward");
            last = v;
        }
    }

    [Test]
    public void ConeIsFullAtTheHackAndHollowAtTheFarEndWhereTheAimBandIsSolid()
    {
        Assert.Greater(AimFuzzBands.ConeProfile(0f, 0f), 0.95f, "starting out, the middle is filled");
        Assert.Less(AimFuzzBands.ConeProfile(0f, 1f), 0.15f, "at the far end the middle is masked out");
        Assert.Greater(AimFuzzBands.ConeProfile(0.7f, 1f), 0.5f, "while the sides stay");
        Assert.AreEqual(0f, AimFuzzBands.ConeProfile(1f, 1f), 1e-5f, "blurred to nothing at the edge");
    }

    [Test]
    public void NoFuzzNoBands()
    {
        AimFuzz.Enabled = false;
        Assert.AreEqual(0f, AimFuzzBands.AimHalfWidth(0f));
        Assert.AreEqual(0f, AimFuzzBands.WeightHalfHeight(0f));
    }
}
