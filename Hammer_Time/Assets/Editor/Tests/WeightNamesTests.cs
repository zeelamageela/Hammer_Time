using NUnit.Framework;

/// <summary>The weight call shown in the timing panel.</summary>
public class WeightNamesTests
{
    [TestCase(-1f, "Short")]
    [TestCase(1f, "1 - Long Guard")]
    [TestCase(2.5f, "2 - Guard")]
    [TestCase(4f, "3 - Tight Guard")]
    [TestCase(5f, "4 - Top 12")]
    [TestCase(5.5f, "5 - Top 8")]
    [TestCase(6f, "6 - Top 4")]
    [TestCase(6.5f, "7 - Button")]
    [TestCase(7f, "8 - Back 4")]
    [TestCase(7.5f, "9 - Back 8")]
    [TestCase(8f, "10 - Back 12")]
    [TestCase(9f, "Hack")]
    [TestCase(11f, "Board")]
    [TestCase(16f, "Takeout")]
    [TestCase(25f, "Peel")]
    public void NamesTheStoppingSpot(float y, string expected)
    {
        Assert.AreEqual(expected, WeightNames.For(y));
    }
}
