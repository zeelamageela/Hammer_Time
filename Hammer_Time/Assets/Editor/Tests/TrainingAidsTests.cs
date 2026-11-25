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
        Assert.IsTrue(TrainingAids.AimCircleUnlocked);
        Assert.IsTrue(TrainingAids.CollisionIndicatorUnlocked);
        Assert.IsTrue(TrainingAids.CollisionWarningUnlocked);
        Assert.AreEqual(1f, TrainingAids.CarvingStrength);
        Assert.AreEqual(0f, TrainingAids.CollisionErrorDegrees);
        Assert.Less(TrainingAids.AimCircleDiameter, 0f, "ungated leaves the circle's size alone");
    }

    [Test]
    public void UntrainedPlayerHasNoAids()
    {
        TrainingAids.Use(new int[5]);
        Assert.IsTrue(TrainingAids.Gated);
        Assert.IsFalse(TrainingAids.AimCircleUnlocked);
        Assert.IsFalse(TrainingAids.CollisionIndicatorUnlocked);
        Assert.IsFalse(TrainingAids.CollisionWarningUnlocked);
        Assert.AreEqual(0f, TrainingAids.CarvingStrength);
    }

    [Test]
    public void TrainedPlayerGetsWhatTheyBought()
    {
        // Directional 2, Tactics 3, On-Ice 3
        TrainingAids.Use(new[] { 0, 2, 3, 3, 0 });
        Assert.IsTrue(TrainingAids.AimCircleUnlocked);
        Assert.AreEqual(0.75f, TrainingAids.AimCircleDiameter);
        Assert.IsTrue(TrainingAids.CollisionIndicatorUnlocked);
        Assert.IsTrue(TrainingAids.CollisionWarningUnlocked, "warning line comes with Tactic Studies 3");
        Assert.AreEqual(6f, TrainingAids.CollisionErrorDegrees);
        Assert.AreEqual(0.75f, TrainingAids.CarvingStrength);

        TrainingAids.Use(new[] { 0, 0, 2, 0, 0 });
        Assert.IsFalse(TrainingAids.CollisionWarningUnlocked);
        Assert.AreEqual(12f, TrainingAids.CollisionErrorDegrees);
    }
}
