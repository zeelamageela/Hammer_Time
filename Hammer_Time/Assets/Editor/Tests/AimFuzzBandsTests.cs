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
    public void PatchIsDenseInTheMiddleAndSoftAtTheEdge()
    {
        Assert.AreEqual(1f, AimFuzzBands.PatchProfile(0f));
        Assert.AreEqual(0f, AimFuzzBands.PatchProfile(1f), 1e-5f);
        Assert.AreEqual(0f, AimFuzzBands.PatchProfile(1.5f), "nothing outside the edge");
        float last = 1f;
        for (float r = 0.1f; r <= 1f; r += 0.05f)
        {
            float v = AimFuzzBands.PatchProfile(r);
            Assert.LessOrEqual(v, last + 1e-5f, "only ever fades outward");
            last = v;
        }
    }

    [Test]
    public void ConeIsDarkestInTheMiddleAndBlursOutToNothing()
    {
        Assert.AreEqual(1f, AimFuzzBands.ConeProfile(0f, 0.5f));
        Assert.Greater(AimFuzzBands.ConeProfile(0.3f, 0.5f), AimFuzzBands.ConeProfile(0.7f, 0.5f));
        Assert.AreEqual(0f, AimFuzzBands.ConeProfile(1f, 0.5f), 1e-5f);
        Assert.AreEqual(AimFuzzBands.ConeProfile(0.5f, 0f), AimFuzzBands.ConeProfile(0.5f, 1f), "the same all along its length");
    }

    [Test]
    public void AimCircleHasASolidThickEdgeAndAFaintFill()
    {
        Assert.AreEqual(0.2f, AimFuzzBands.RingProfile(0f), 1e-5f, "faint fill");
        Assert.AreEqual(0.2f, AimFuzzBands.RingProfile(0.5f), 1e-5f);
        Assert.AreEqual(1f, AimFuzzBands.RingProfile(0.94f), 1e-5f, "the thick edge is solid");
        Assert.AreEqual(0f, AimFuzzBands.RingProfile(1.05f), 1e-5f, "nothing outside the circle");
    }

    [Test]
    public void NoFuzzNoBands()
    {
        AimFuzz.Enabled = false;
        Assert.AreEqual(0f, AimFuzzBands.AimHalfWidth(0f));
        Assert.AreEqual(0f, AimFuzzBands.WeightHalfHeight(0f));
    }
}
