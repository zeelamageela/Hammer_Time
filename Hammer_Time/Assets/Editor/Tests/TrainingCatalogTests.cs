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
        Assert.AreEqual("Sweep Drills", TrainingCatalog.Next(owned, TrainingTrack.Sweep).name);
    }

    [Test]
    public void ShortOrMissingSavesCountAsNothingOwned()
    {
        Assert.AreEqual(0, TrainingCatalog.Owned(null, TrainingTrack.Cross));
        Assert.AreEqual(0, TrainingCatalog.Owned(new[] { 2 }, TrainingTrack.Cross));
        Assert.AreEqual(2, TrainingCatalog.Owned(new[] { 2 }, TrainingTrack.Sweep));
        Assert.AreEqual(4, TrainingCatalog.Owned(new[] { 99 }, TrainingTrack.Sweep), "clamped");
        var grown = TrainingCatalog.WithLevelBought(new[] { 1 }, TrainingTrack.Cross);
        Assert.AreEqual(TrainingCatalog.Tracks, grown.Length);
        Assert.AreEqual(1, grown[0]);
        Assert.AreEqual(1, grown[(int)TrainingTrack.Cross]);
    }

    [Test]
    public void StatBonusesAreCumulative()
    {
        Assert.AreEqual(0, TrainingCatalog.StatBonus(null).sweepEndurance);

        // Sweep levels 1-2 (+4 endurance, +5 strength) plus all of cross training (jogging 4, yoga 3, rowing 4 endurance/strength...)
        var owned = new[] { 2, 0, 0, 0, 4 };
        var b = TrainingCatalog.StatBonus(owned);
        Assert.AreEqual(4 + 4 + 3, b.sweepEndurance);
        Assert.AreEqual(5 + 3 + 4, b.sweepStrength);
        Assert.AreEqual(4, b.sweepCohesion);
        Assert.AreEqual(4, b.aimAccuracy);
        Assert.AreEqual(4, b.finesseAccuracy);
    }

    [Test]
    public void UnlockTracksTightenWithEveryLevel()
    {
        float lastCircle = float.MaxValue, lastError = float.MaxValue, lastCarve = -1f;
        for (int l = 1; l <= 4; l++)
        {
            var owned = new[] { 0, l, l, l, 0 };
            float circle = TrainingCatalog.AimCircleDiameter(owned);
            float error = TrainingCatalog.CollisionIndicatorError(owned);
            float carve = TrainingCatalog.DirectionalStrength(owned);
            Assert.Less(circle, lastCircle, "aim circle narrows");
            Assert.Less(error, lastError, "collision indicator sharpens");
            Assert.Greater(carve, lastCarve, "carving strengthens");
            lastCircle = circle; lastError = error; lastCarve = carve;
        }
        Assert.AreEqual(0f, TrainingCatalog.AimCircleDiameter(null), "no training, no aim circle");
        Assert.Less(TrainingCatalog.CollisionIndicatorError(null), 0f, "no training, no indicator");
        Assert.AreEqual(0f, TrainingCatalog.DirectionalStrength(null), "no training, no carving");
        Assert.AreEqual(0.75f, TrainingCatalog.AimCircleDiameter(new[] { 0, 0, 0, 3, 0 }), "half of the 1.5 start, at level 3 (roughly)");
    }
}
