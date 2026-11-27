using NUnit.Framework;

/// <summary>How the computer learns the ice.</summary>
public class AiReadTests
{
    [Test]
    public void ItStartsAtTheAverageAndClosesOnTheTruth()
    {
        Assert.AreEqual(1f, AiRead.Read(1f, 1.1f, 1f, 0f, 0.5f, AiRead.StubbornSpeed), 1e-4f);
        float last = 1f;
        for (int rocks = 1; rocks <= 20; rocks++)
        {
            float read = AiRead.Read(1f, 1.1f, 1f, rocks, 0.5f, AiRead.StubbornSpeed);
            Assert.Greater(read, last);
            last = read;
        }
        Assert.AreEqual(1.1f, last, 0.001f);
    }

    [Test]
    public void ASkilledTeamLearnsFaster()
    {
        float good = AiRead.Read(1f, 1.1f, 0.9f, 6f, 0f, AiRead.StubbornSpeed);
        float poor = AiRead.Read(1f, 1.1f, 0.3f, 6f, 0f, AiRead.StubbornSpeed);
        Assert.Less(1.1f - good, 1.1f - poor);
    }

    [Test]
    public void AWeakTeamNeverGetsItQuiteRight()
    {
        float weak = AiRead.Read(1f, 1.1f, 0.2f, 500f, 1f, AiRead.StubbornSpeed);
        Assert.AreEqual(1.1f + AiRead.StubbornSpeed * 0.8f, weak, 1e-4f);
    }
}
