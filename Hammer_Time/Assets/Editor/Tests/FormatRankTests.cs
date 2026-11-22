using NUnit.Framework;

public class FormatRankTests
{
    [TestCase(1, "1st")]
    [TestCase(2, "2nd")]
    [TestCase(3, "3rd")]
    [TestCase(4, "4th")]
    [TestCase(11, "11th")]
    [TestCase(12, "12th")]
    [TestCase(13, "13th")]
    [TestCase(21, "21st")]
    [TestCase(22, "22nd")]
    [TestCase(23, "23rd")]
    [TestCase(111, "111th")]
    public void FormatRank_UsesTheRightSuffix(int rank, string expected)
    {
        Assert.AreEqual(expected, TextReplacementUtility.FormatRank(rank));
    }
}
