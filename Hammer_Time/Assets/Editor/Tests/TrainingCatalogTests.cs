using NUnit.Framework;

public class TrainingCatalogTests
{
    [Test]
    public void EveryTrackHasFourNamedLevelsAtTheFixedPrices()
    {
        for (int t = 0; t < TrainingCatalog.Tracks; t++)
            for (int l = 1; l <= TrainingCatalog.MaxLevel; l++)
            {
                var lv = TrainingCatalog.Level((TrainingTrack)t, l);
                Assert.IsNotNull(lv, "track " + t + " level " + l);
                Assert.IsFalse(string.IsNullOrEmpty(lv.name));
                Assert.IsFalse(string.IsNullOrEmpty(lv.effect));
                Assert.AreEqual(TrainingCatalog.LevelCost[l - 1], lv.cost);
            }
        Assert.AreEqual(3, TrainingCatalog.Tracks);
        Assert.IsNull(TrainingCatalog.Level(TrainingTrack.Sweep, 0));
        Assert.IsNull(TrainingCatalog.Level(TrainingTrack.Sweep, 5));
    }

    [Test]
    public void BuyingRaisesOneTrackAndStopsAtLevelFour()
    {
        int[] owned = null;
        for (int i = 0; i < 6; i++) owned = TrainingCatalog.WithLevelBought(owned, TrainingTrack.OnIce);
        Assert.AreEqual(4, TrainingCatalog.Owned(owned, TrainingTrack.OnIce));
        Assert.AreEqual(0, TrainingCatalog.Owned(owned, TrainingTrack.Sweep));
        Assert.IsNull(TrainingCatalog.Next(owned, TrainingTrack.OnIce));
        Assert.AreEqual("Directional Sweeping", TrainingCatalog.Next(owned, TrainingTrack.Sweep).name);
    }

    [Test]
    public void ShortOrMissingSavesCountAsNothingOwned()
    {
        Assert.AreEqual(0, TrainingCatalog.Owned(null, TrainingTrack.OnIce));
        Assert.AreEqual(0, TrainingCatalog.Owned(new[] { 2 }, TrainingTrack.OnIce));
        Assert.AreEqual(2, TrainingCatalog.Owned(new[] { 2 }, TrainingTrack.Sweep));
        Assert.AreEqual(4, TrainingCatalog.Owned(new[] { 99 }, TrainingTrack.Sweep), "clamped");
        var grown = TrainingCatalog.WithLevelBought(new[] { 1 }, TrainingTrack.OnIce);
        Assert.AreEqual(TrainingCatalog.Tracks, grown.Length);
        Assert.AreEqual(1, grown[0]);
        Assert.AreEqual(1, grown[(int)TrainingTrack.OnIce]);
    }

    [Test]
    public void StatBonusesAreCumulative()
    {
        Assert.AreEqual(0, TrainingCatalog.StatBonus(null).sweepEndurance);

        // Sweep 1-3 (carving, +4 endurance, +5 strength) and all of On-Ice (+6 weight, +3 aim)
        var b = TrainingCatalog.StatBonus(new[] { 3, 0, 4 });
        Assert.AreEqual(4, b.sweepEndurance);
        Assert.AreEqual(5, b.sweepStrength);
        Assert.AreEqual(0, b.sweepCohesion, "cohesion is level 4");
        Assert.AreEqual(6, b.weightAccuracy);
        Assert.AreEqual(3, b.aimAccuracy);
        Assert.AreEqual(5, TrainingCatalog.StatBonus(new[] { 4, 0, 0 }).sweepCohesion);
    }

    [Test]
    public void AidsComeInTheAgreedOrder()
    {
        var none = new int[3];
        Assert.IsFalse(TrainingCatalog.CarvingUnlocked(none));
        Assert.IsTrue(TrainingCatalog.CarvingUnlocked(new[] { 1, 0, 0 }));

        Assert.IsFalse(TrainingCatalog.CrosshairsUnlocked(none));
        Assert.IsTrue(TrainingCatalog.CrosshairsUnlocked(new[] { 0, 1, 0 }));
        Assert.IsFalse(TrainingCatalog.CollisionWarningUnlocked(new[] { 0, 1, 0 }));
        Assert.IsTrue(TrainingCatalog.CollisionWarningUnlocked(new[] { 0, 2, 0 }));
        Assert.Less(TrainingCatalog.CollisionIndicatorError(new[] { 0, 2, 0 }), 0f, "no hit arrows until level 3");
        Assert.AreEqual(12f, TrainingCatalog.CollisionIndicatorError(new[] { 0, 3, 0 }));
        Assert.AreEqual(0f, TrainingCatalog.CollisionIndicatorError(new[] { 0, 4, 0 }), "exact");

        Assert.IsFalse(TrainingCatalog.DotsUnlocked(none));
        Assert.IsTrue(TrainingCatalog.DotsUnlocked(new[] { 0, 0, 1 }));
        Assert.IsFalse(TrainingCatalog.TimingUnlocked(new[] { 0, 0, 1 }));
        Assert.IsTrue(TrainingCatalog.TimingUnlocked(new[] { 0, 0, 2 }));
    }

    [Test]
    public void TheTrajectoryLineReachesFurtherWithEachOnIceLevel()
    {
        float last = float.MinValue;
        for (int l = 0; l <= 4; l++)
        {
            float reach = TrainingCatalog.LineReachY(new[] { 0, 0, l });
            Assert.GreaterOrEqual(reach, last);
            last = reach;
        }
        Assert.AreEqual(0f, TrainingCatalog.LineReachY(new[] { 0, 0, 1 }), "the far hog line");
        Assert.AreEqual(0f, TrainingCatalog.LineReachY(new[] { 0, 0, 2 }), "level 2 is the timing ranges, not more line");
        Assert.Greater(TrainingCatalog.LineReachY(new[] { 0, 0, 3 }), 0f);
        Assert.IsTrue(float.IsPositiveInfinity(TrainingCatalog.LineReachY(new[] { 0, 0, 4 })));
    }

    [Test]
    public void OnlyTheFirstLevelOfATrackNeedsAPlayerLevel()
    {
        for (int t = 0; t < TrainingCatalog.Tracks; t++)
        {
            Assert.AreEqual(TrainingCatalog.FirstLevelGate, TrainingCatalog.Level((TrainingTrack)t, 1).requiredLevel);
            for (int l = 2; l <= 4; l++) Assert.AreEqual(1, TrainingCatalog.Level((TrainingTrack)t, l).requiredLevel);
        }

        var none = new int[3];
        Assert.IsFalse(TrainingCatalog.LevelAllows(none, TrainingTrack.OnIce, 1), "the first level needs player level 2");
        Assert.IsTrue(TrainingCatalog.LevelAllows(none, TrainingTrack.OnIce, 2));
        Assert.IsTrue(TrainingCatalog.LevelAllows(new[] { 0, 0, 1 }, TrainingTrack.OnIce, 1), "after that it is only cash");
        Assert.IsFalse(TrainingCatalog.LevelAllows(new[] { 4, 0, 0 }, TrainingTrack.Sweep, 99), "nothing left to buy");
    }

    [Test]
    public void OldFiveTrackSavesAreMovedOver()
    {
        // old order: sweep, directional, tactics, on-ice, cross
        Assert.AreEqual(new[] { 3, 2, 4 }, TrainingCatalog.MigrateLegacy(new[] { 2, 1, 2, 4, 4 }), "carving became sweep level 1, so sweep moves up one");
        Assert.AreEqual(new[] { 0, 1, 0 }, TrainingCatalog.MigrateLegacy(new[] { 3, 0, 1, 0, 0 }), "no carving: the sweep levels are not carried");
        Assert.AreEqual(new[] { 4, 0, 0 }, TrainingCatalog.MigrateLegacy(new[] { 4, 4, 0, 0, 0 }), "capped at level 4");
        var current = new[] { 1, 2, 3 };
        Assert.AreSame(current, TrainingCatalog.MigrateLegacy(current), "three tracks are already current");
        Assert.IsNull(TrainingCatalog.MigrateLegacy(null));
    }
}
