using System.Collections.Generic;
using NUnit.Framework;

public class ClubNightTests
{
    static Team Make(string name, int wins, int difference, int scored, bool player = false)
    {
        return new Team { name = name, player = player, wins = wins, pointDifferential = difference, pointsFor = scored };
    }

    [Test]
    public void EveryonePlaysEveryoneOnce()
    {
        var met = new HashSet<string>();
        foreach (var draw in ClubNight.Schedule())
        {
            var busy = new HashSet<int>();
            foreach (var g in draw.game)
            {
                Assert.IsTrue(busy.Add(g.x) && busy.Add(g.y), "a team plays once a draw");
                Assert.IsTrue(met.Add(System.Math.Min(g.x, g.y) + "-" + System.Math.Max(g.x, g.y)), "no rematch");
            }
            Assert.AreEqual(ClubNight.Teams, busy.Count);
        }
        Assert.AreEqual(6, met.Count);
        Assert.AreEqual(ClubNight.Draws, ClubNight.Schedule().Length);
    }

    [Test]
    public void PotIsTheEntryFeesUnlessAPrizeIsListed()
    {
        var t = new Tourny { entryFee = 500 };
        ClubNight.Prepare(t);
        Assert.IsTrue(t.clubNight);
        Assert.AreEqual(2000, t.prizeMoney);
        ClubNight.Prepare(t);
        Assert.AreEqual(2000, t.prizeMoney, "preparing again changes nothing");

        var listed = new Tourny { entryFee = 500, prizeMoney = 3500 };
        ClubNight.Prepare(listed);
        Assert.AreEqual(3500, listed.prizeMoney);
    }

    [Test]
    public void ClubNightsTakeTurns()
    {
        Assert.AreEqual(1, ClubNight.Pick(4, 3));
        Assert.AreEqual(2, ClubNight.Pick(5, 3));
        Assert.AreEqual(0, ClubNight.Pick(6, 3));
        Assert.AreEqual(0, ClubNight.Pick(6, 0));
    }

    [Test]
    public void TiesGoToPointDifferenceThenPointsScored()
    {
        var a = Make("A", 2, 3, 15);
        var b = Make("B", 2, 5, 12);
        var c = Make("C", 2, 5, 14);
        var d = Make("D", 0, 9, 30);
        var winners = ClubNight.Winners(new[] { a, b, c, d });
        Assert.AreEqual(1, winners.Count);
        Assert.AreSame(c, winners[0]);
    }

    [Test]
    public void TeamsExactlyLevelShareThePot()
    {
        var a = Make("A", 2, 4, 14, true);
        var b = Make("B", 2, 4, 14);
        var c = Make("C", 1, 8, 20);
        var winners = ClubNight.Winners(new[] { c, a, b });
        Assert.AreEqual(2, winners.Count);
        Assert.AreEqual(1000f, ClubNight.Share(2000f, winners));
        Assert.AreEqual(0f, ClubNight.Share(2000f, new[] { c }));
    }

    [Test]
    public void ScoresCountForBothTeams()
    {
        var a = Make("A", 0, 0, 0);
        var b = Make("B", 0, 0, 0);
        ClubNight.AddScore(a, b, 7, 4);
        Assert.AreEqual(3, a.pointDifferential);
        Assert.AreEqual(-3, b.pointDifferential);
        Assert.AreEqual(7, a.pointsFor);
        Assert.AreEqual(4, b.pointsFor);
    }

    [Test]
    public void SimulatedScoresAgreeWithTheWinner()
    {
        var random = new System.Random(1);
        for (int i = 0; i < 200; i++)
        {
            bool aWon = i % 2 == 0;
            ClubNight.SimScore(aWon, random.Next, out int a, out int b);
            Assert.AreEqual(aWon, a > b);
            Assert.AreNotEqual(a, b);
        }
    }

    [Test]
    public void StandingsPutMorePointsScoredFirst()
    {
        var list = new List<Team_List> { new Team_List(Make("A", 2, 4, 10)), new Team_List(Make("B", 2, 4, 18)) };
        list.Sort();
        Assert.AreEqual("B", list[0].team.name);
    }
}
