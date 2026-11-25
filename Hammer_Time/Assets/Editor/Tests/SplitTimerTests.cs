using NUnit.Framework;

/// <summary>The split clock's pure parts: the 00:00 format and the sub-step line crossing.</summary>
public class SplitTimerTests
{
    [TestCase(0f, "00:00")]
    [TestCase(3.21f, "03:21")]
    [TestCase(3.2149f, "03:21")]
    [TestCase(0.05f, "00:05")]
    [TestCase(14.35f, "14:35")]
    [TestCase(61.999f, "61:99")]
    [TestCase(150f, "99:99")]
    [TestCase(-1f, "00:00")]
    public void FormatsSecondsAndHundredths(float seconds, string expected)
    {
        Assert.AreEqual(expected, SplitTimer.Format(seconds));
    }

    [Test]
    public void CrossingIsInterpolatedInsideTheStep()
    {
        // 0.02 s step from y=-16.20 to y=-16.10 crosses -16.15 exactly half way
        Assert.AreEqual(10.01f, SplitTimer.CrossingTime(-16.20f, 10.00f, -16.10f, 10.02f, -16.15f), 1e-5f);
        // landing exactly on the line
        Assert.AreEqual(10.02f, SplitTimer.CrossingTime(-16.20f, 10.00f, -16.15f, 10.02f, -16.15f), 1e-5f);
        // a rock that did not move cannot divide by zero
        Assert.AreEqual(10.02f, SplitTimer.CrossingTime(-16.2f, 10.00f, -16.2f, 10.02f, -16.15f), 1e-5f);
    }

    [Test]
    public void SplitsAddUpToTheWholeTrip()
    {
        // a rock at 9 m/s crossing the hack, near hog and far hog: the two clocks must be the two gaps between the crossings
        float v = 9f;
        float tHack = 0.0137f;
        float tHog = tHack + (SplitTimer.NearHogY - SplitTimer.HackY) / v;
        float tFar = tHog + (SplitTimer.FarHogY - SplitTimer.NearHogY) / v;
        // sample at 0.02 s like physics does
        float Cross(float line)
        {
            float prevY = SplitTimer.HackY - 3f, prevT = tHack - 3f / v;
            for (float t = prevT + 0.02f; t < 10f; t += 0.02f)
            {
                float y = SplitTimer.HackY + (t - tHack) * v;
                if (prevY < line && y >= line) return SplitTimer.CrossingTime(prevY, prevT, y, t, line);
                prevY = y; prevT = t;
            }
            return -1f;
        }
        Assert.AreEqual(tHog - tHack, Cross(SplitTimer.NearHogY) - Cross(SplitTimer.HackY), 1e-4f);
        Assert.AreEqual(tFar - tHog, Cross(SplitTimer.FarHogY) - Cross(SplitTimer.NearHogY), 1e-4f);
    }
}
