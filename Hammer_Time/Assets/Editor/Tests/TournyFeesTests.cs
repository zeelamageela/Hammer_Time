using NUnit.Framework;

public class TournyFeesTests
{
    [Test]
    public void Total_IsEntryFeePlusWeeklyCost()
    {
        var bigBen = new Tourny { name = "Big Ben's Big Cash $piel", entryFee = 10000 };

        Assert.AreEqual(10500f, TournyFees.Total(bigBen, 500f));
    }

    [Test]
    public void ChargeThenRefund_ReturnsExactlyTheStartingCash()
    {
        var t = new Tourny { entryFee = 1000 };
        float cash = 5000f;

        cash -= TournyFees.Total(t, 250f);     // PlayTourny
        cash += TournyFees.Total(t, 250f);     // Back

        Assert.AreEqual(5000f, cash);
    }

    [Test]
    public void FreeEvent_CostsOnlyTheWeeklyCost()
    {
        Assert.AreEqual(250f, TournyFees.Total(new Tourny { entryFee = 0 }, 250f));
    }

    [Test]
    public void CanAfford_CountsTheWeeklyCostNotJustTheFee()
    {
        var t = new Tourny { entryFee = 1000 };

        Assert.IsFalse(TournyFees.CanAfford(t, 300f, 1200f), "fee alone fits, fee + weekly cost does not");
        Assert.IsTrue(TournyFees.CanAfford(t, 300f, 1300f), "exactly the total is affordable");
        Assert.IsFalse(TournyFees.CanAfford(t, 0f, -9575f), "negative cash can never afford an event");
    }

    [Test]
    public void AnyAffordable_OneAffordableEventKeepsTheRunAlive()
    {
        var empty = new Tourny();
        var pricey = new Tourny { entryFee = 5000 };
        var cheap = new Tourny { entryFee = 100 };

        Assert.IsTrue(TournyFees.AnyAffordable(new[] { empty, pricey, cheap }, empty, 300f, 500f));
        Assert.IsFalse(TournyFees.AnyAffordable(new[] { empty, pricey, cheap }, empty, 300f, 399f), "nothing fits: game over");
        Assert.IsFalse(TournyFees.AnyAffordable(new[] { cheap }, empty, 300f, -9575f), "the in-the-hole save");
    }

    [Test]
    public void AnyAffordable_EmptyBoardIsNotBankruptcy()
    {
        var empty = new Tourny();

        Assert.IsTrue(TournyFees.AnyAffordable(new[] { empty, empty, empty }, empty, 300f, 0f));
        Assert.IsTrue(TournyFees.AnyAffordable(null, empty, 300f, 0f));
    }

    [Test]
    public void NullEvent_IsTolerated()
    {
        Assert.AreEqual(250f, TournyFees.Total(null, 250f));
    }
}
