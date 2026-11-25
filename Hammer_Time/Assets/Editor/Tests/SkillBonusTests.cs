using NUnit.Framework;

public class SkillBonusTests
{
    static Equipment Item(params int[] stats) { return new Equipment { stats = stats }; }

    [Test]
    public void GearBonusIsTheSumOfTheEquippedItems_InStatOrder()
    {
        // stats order: weight, finesse, aim, endurance, strength, cohesion
        var sum = EquipmentManager.SumBonus(new[] { Item(1, 2, 3, 4, 5, 6), Item(10, 20, 30, 40, 50, 60), null, Item(1, 1) });
        Assert.AreEqual(11, sum.weightAccuracy);
        Assert.AreEqual(22, sum.finesseAccuracy);
        Assert.AreEqual(33, sum.aimAccuracy);
        Assert.AreEqual(44, sum.sweepEndurance);
        Assert.AreEqual(55, sum.sweepStrength);
        Assert.AreEqual(66, sum.sweepCohesion);
        Assert.AreEqual(0, EquipmentManager.SumBonus(null).aimAccuracy);
    }

    [Test]
    public void AskingTwiceDoesNotPileUp()
    {
        var kit = new[] { Item(0, 0, 5, 0, 0, 0) };
        Assert.AreEqual(5, EquipmentManager.SumBonus(kit).aimAccuracy);
        Assert.AreEqual(5, EquipmentManager.SumBonus(kit).aimAccuracy, "recomputed from the items, not added to a running total");
    }

    [Test]
    public void GearAndTrainingAddUp()
    {
        var gear = new CareerStats { aimAccuracy = 7, sweepStrength = 3 };
        var training = TrainingCatalog.StatBonus(new[] { 2, 0, 0, 0, 4 });   // sweep 1-2, all of cross
        var both = SkillBonuses.Combine(gear, training);
        Assert.AreEqual(7 + training.aimAccuracy, both.aimAccuracy);
        Assert.AreEqual(3 + training.sweepStrength, both.sweepStrength);
        Assert.AreEqual(0, SkillBonuses.Combine(null, null).sweepEndurance);
    }
}
