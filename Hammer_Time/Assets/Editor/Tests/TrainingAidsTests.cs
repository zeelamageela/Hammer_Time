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
    public void ReachCountsThePointsUpToTheCap()
    {
        var path = new System.Collections.Generic.List<UnityEngine.Vector3>();
        for (int i = 0; i <= 20; i++) path.Add(new UnityEngine.Vector3(0f, -20f + i * 2f, 0f));   // y = -20 .. 20
        Assert.AreEqual(21, TrainingAids.CountWithinReach(path, float.PositiveInfinity));
        Assert.AreEqual(11, TrainingAids.CountWithinReach(path, 0f), "points with y <= 0");
        Assert.AreEqual(2, TrainingAids.CountWithinReach(path, -100f), "a line always keeps at least two points");
        Assert.AreEqual(0, TrainingAids.CountWithinReach(null, 0f));
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
