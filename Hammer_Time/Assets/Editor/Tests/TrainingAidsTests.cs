using NUnit.Framework;

public class TrainingAidsTests
{
    [TearDown]
    public void Ungate() { TrainingAids.Use(null); }

    [Test]
    public void UngatedMatchesGetEverythingAsBefore()
    {
        TrainingAids.Use(null);
        Assert.IsFalse(TrainingAids.Gated);
        Assert.IsTrue(TrainingAids.CrosshairsUnlocked);
        Assert.IsTrue(TrainingAids.CollisionIndicatorUnlocked);
        Assert.IsTrue(TrainingAids.CollisionWarningUnlocked);
        Assert.IsTrue(TrainingAids.CarvingUnlocked);
        Assert.IsTrue(TrainingAids.DotsUnlocked);
        Assert.IsTrue(TrainingAids.TimingUnlocked);
        Assert.IsTrue(float.IsPositiveInfinity(TrainingAids.LineReachY));
        Assert.AreEqual(0f, TrainingAids.CollisionErrorDegrees);
    }

    [Test]
    public void UntrainedPlayerHasNoAids()
    {
        TrainingAids.Use(new int[TrainingCatalog.Tracks]);
        Assert.IsTrue(TrainingAids.Gated);
        Assert.IsFalse(TrainingAids.CrosshairsUnlocked);
        Assert.IsFalse(TrainingAids.CollisionIndicatorUnlocked);
        Assert.IsFalse(TrainingAids.CollisionWarningUnlocked);
        Assert.IsFalse(TrainingAids.CarvingUnlocked);
        Assert.IsFalse(TrainingAids.DotsUnlocked);
        Assert.IsFalse(TrainingAids.TimingUnlocked);
        Assert.Less(TrainingAids.LineReachY, 0f, "just the near end of the line");
    }

    [Test]
    public void TrainedPlayerGetsWhatTheyBought()
    {
        // Sweep 1, Tactics 3, On-Ice 3
        TrainingAids.Use(new[] { 1, 3, 3 });
        Assert.IsTrue(TrainingAids.CarvingUnlocked);
        Assert.IsTrue(TrainingAids.CrosshairsUnlocked);
        Assert.IsTrue(TrainingAids.CollisionWarningUnlocked);
        Assert.IsTrue(TrainingAids.CollisionIndicatorUnlocked);
        Assert.AreEqual(12f, TrainingAids.CollisionErrorDegrees);
        Assert.IsTrue(TrainingAids.DotsUnlocked);
        Assert.IsTrue(TrainingAids.TimingUnlocked);
        Assert.Greater(TrainingAids.LineReachY, 0f);
        Assert.IsFalse(float.IsPositiveInfinity(TrainingAids.LineReachY));

        TrainingAids.Use(new[] { 0, 2, 0 });
        Assert.IsTrue(TrainingAids.CollisionWarningUnlocked);
        Assert.IsFalse(TrainingAids.CollisionIndicatorUnlocked, "hit arrows are level 3");
    }
}
