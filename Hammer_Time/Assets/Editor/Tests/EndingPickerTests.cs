using NUnit.Framework;

public class EndingPickerTests
{
    static EndingPicker.Facts Base(int qualification = EndingPicker.BothChamps) => new EndingPicker.Facts
    {
        qualification = qualification,
        earnings = 50000f,
        bestLeagueRival = 80000f,
        bestRecord = 300000f,
    };

    [Test]
    public void Qualification_MapsTheOriginalFiveEndings()
    {
        Assert.AreEqual(0, EndingPicker.Qualification(provQual: false, tourQual: false, champQual: false));
        Assert.AreEqual(1, EndingPicker.Qualification(provQual: true, tourQual: false, champQual: false));
        Assert.AreEqual(2, EndingPicker.Qualification(provQual: true, tourQual: true, champQual: false));
        Assert.AreEqual(3, EndingPicker.Qualification(provQual: true, tourQual: false, champQual: true));
        Assert.AreEqual(4, EndingPicker.Qualification(provQual: true, tourQual: true, champQual: true));
    }

    [Test]
    public void Pick_WithNothingSpecial_KeepsTheQualificationEnding()
    {
        Assert.AreEqual(EndingPicker.TourChampOnly, EndingPicker.Pick(Base(EndingPicker.TourChampOnly)));
    }

    [Test]
    public void Pick_WonBothTitles_BeatsEverything()
    {
        var f = Base();
        f.wonTourTitle = true;
        f.wonProvTitle = true;
        f.earnings = 999999f;

        Assert.AreEqual(EndingPicker.WonBothTitles, EndingPicker.Pick(f));
    }

    [Test]
    public void Pick_WonOneTitle_ReportsWhichOne()
    {
        var f = Base();
        f.wonTourTitle = true;
        Assert.AreEqual(EndingPicker.WonTourTitle, EndingPicker.Pick(f));

        f = Base();
        f.wonProvTitle = true;
        Assert.AreEqual(EndingPicker.WonProvTitle, EndingPicker.Pick(f));
    }

    [Test]
    public void Pick_MoneyRecord_BeatsASingleTitle()
    {
        var f = Base();
        f.wonTourTitle = true;
        f.earnings = 400000f;

        Assert.AreEqual(EndingPicker.MoneyRecord, EndingPicker.Pick(f));
    }

    [Test]
    public void Pick_NoLeaderboardYet_IsNotARecord()
    {
        var f = Base();
        f.bestRecord = 0f;
        f.earnings = 10000f;
        f.bestLeagueRival = 20000f;

        Assert.AreEqual(f.qualification, EndingPicker.Pick(f), "the first career ever shouldn't count as a record");
    }

    [Test]
    public void Pick_LeadingTheLeagueInMoney_NeedsToBeatEveryRival()
    {
        var f = Base();
        f.earnings = 90000f;
        Assert.AreEqual(EndingPicker.MoneyLeader, EndingPicker.Pick(f));

        f.earnings = 80000f;
        Assert.AreEqual(f.qualification, EndingPicker.Pick(f), "a tie is not leading");
    }

    [Test]
    public void Pick_BigMoneyWithoutLeadingTheLeague()
    {
        var f = Base();
        f.bestLeagueRival = 1000000f;
        f.earnings = EndingPicker.BigMoneyEarnings;

        Assert.AreEqual(EndingPicker.BigMoney, EndingPicker.Pick(f));
    }

    [Test]
    public void Pick_ZeroEarningsNeverLeadsTheLeague()
    {
        var f = Base();
        f.earnings = 0f;
        f.bestLeagueRival = 0f;

        Assert.AreEqual(f.qualification, EndingPicker.Pick(f));
    }
}
