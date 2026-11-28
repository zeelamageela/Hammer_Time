using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class StandingsTests
{
    static Team T(int id, int wins = 0, int losses = 0, float earnings = 0, float points = 0) => new Team
        { id = id, name = "T" + id, seasonWins = wins, seasonLosses = losses, seasonEarnings = earnings, tourPoints = points };

    [Test]
    public void MoneyThenRecordThenName()
    {
        var table = Standings.Money(new[] { T(1, 1, 0, 100), T(2, 0, 3, 500), T(3, 4, 0, 100), T(4, 4, 2, 100), T(5, 1, 0, 100) });
        CollectionAssert.AreEqual(new[] { 2, 3, 4, 1, 5 }, table.Select(t => t.id));
        Assert.AreEqual(2, Standings.Place(table, 3));
        Assert.AreEqual(0, Standings.Place(table, 99));
    }

    [Test]
    public void TourOrdersByPoints()
    {
        var table = Standings.Tour(new[] { T(1, 9, 0, 900, 5), T(2, 0, 1, 0, 25) });
        CollectionAssert.AreEqual(new[] { 2, 1 }, table.Select(t => t.id));
    }

    [Test]
    public void HistoryFollowsTheTeamNotItsPosition()
    {
        var played = new[] { T(2, 1, 0, 50), T(1, 0, 1, 10) };   // reordered since the history was taken
        Standings.AddHistory(played, new[] { new Vector4(5, 4, 1000, 1), new Vector4(2, 3, 200, 2) });
        Assert.AreEqual(2, played[0].id);
        Assert.AreEqual(3, played[0].seasonWins);
        Assert.AreEqual(250f, played[0].seasonEarnings);
        Assert.AreEqual(5, played[1].seasonWins);
        Assert.AreEqual(5, played[1].seasonLosses);
    }

    [Test]
    public void ResultsReachTheLeagueAndTourCopies()
    {
        var league = new[] { T(1), T(2, 7) };
        var tour = new[] { T(1) };
        Standings.Merge(new[] { T(1, 3, 1, 400, 18) }, league, tour);
        foreach (var t in new[] { league[0], tour[0] })
        {
            Assert.AreEqual(3, t.seasonWins);
            Assert.AreEqual(400f, t.seasonEarnings);
            Assert.AreEqual(18f, t.tourPoints);
        }
        Assert.AreEqual(7, league[1].seasonWins);
    }

    [Test]
    public void TourListBecomesTheLeaguesOwnTeams()
    {
        var league = new[] { T(1, 2), T(2) };
        var tour = new[] { T(2, 0, 0, 0, 12), T(9) };
        Standings.Link(league, tour);
        Assert.AreSame(league[1], tour[0]);
        Assert.AreEqual(12f, league[1].tourPoints);
        Assert.AreEqual(9, tour[1].id);
    }
}
