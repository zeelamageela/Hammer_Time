using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class LeagueTests
{
    static List<League.Entry> Build(string country, int seed, string player = "Tester")
    {
        var random = new System.Random(seed);
        return League.Build(player, country, "Town Club", random.Next);
    }

    static string[] Csv(string file) => File.ReadAllLines(Path.Combine(Application.dataPath, "..", file))
        .Skip(1).Where(l => l.Trim().Length > 0)
        .Select(l => l.Split(',')[0] + ":" + string.Join(" ", l.Split(',').Skip(1).Where(c => c.Length > 0)))
        .ToArray();

    static string[] InGame(League.Name[] names) =>
        names.Select(n => n.name + ":" + string.Join(" ", n.countries)).ToArray();

    [Test]
    public void NamesMatchTheApprovedLists()
    {
        CollectionAssert.AreEqual(Csv("CHARACTERS.csv"), InGame(League.Characters));
        CollectionAssert.AreEqual(Csv("TEAM_NAMES.csv"), InGame(League.Surnames));
    }

    [Test]
    public void TownsMatchTheApprovedList()
    {
        var csv = Csv("TOWNS.csv").Select(l => l.Split(new[] { ':' }, 2)).ToList();
        foreach (var country in League.Countries)
            CollectionAssert.AreEqual(csv.Where(r => r[0] == country).Select(r => r[1]), League.Towns(country));
    }

    [Test]
    public void OtherClubsAreSevenDifferentTowns()
    {
        foreach (var home in League.Countries)
        {
            Assert.AreEqual(8, League.Towns(home).Length, home);
            foreach (var mine in new[] { League.Towns(home)[2].ToUpperInvariant(), "Somewhere Else" })
            {
                var random = new System.Random(4);
                var others = League.Build("Tester", home, mine, random.Next)
                    .Where(e => e.kind == League.Kind.ClubChampion).Select(e => e.club).ToList();
                Assert.AreEqual(7, others.Distinct().Count(), home);
                Assert.IsTrue(others.All(t => League.Towns(home).Contains(t)), home);
                Assert.IsFalse(others.Any(t => t.ToUpperInvariant() == mine), home);
            }
        }
    }

    [Test]
    public void EveryCountryMakesAFullLeague()
    {
        foreach (var home in League.Countries)
            for (int seed = 0; seed < 40; seed++)
            {
                var league = Build(home, seed);
                Assert.AreEqual(32, league.Count);
                Assert.AreEqual(32, league.Select(e => e.name.ToLowerInvariant()).Distinct().Count(), home);
                Assert.AreEqual(League.Kind.Player, league[0].kind);
                Assert.AreEqual(16, league.Count(e => e.country == home), home);
                Assert.AreEqual(9, league.Count(e => e.club == "Town Club"), home);
                Assert.AreEqual(7, league.Count(e => e.kind == League.Kind.ClubChampion), home);
                foreach (var country in League.Countries)
                    Assert.IsTrue(league.Any(e => e.country == country), home + " has nobody from " + country);
            }
    }

    [Test]
    public void FifteenOnTourBesideThePlayer()
    {
        var league = Build("SCO", 3);
        Assert.IsFalse(league[0].onTour);
        Assert.AreEqual(League.TourTeams - 1, league.Count(e => e.onTour));
        Assert.AreEqual(2, league.Count(e => e.onTour && e.kind == League.Kind.Clubmate));
        Assert.AreEqual(4, league.Count(e => e.onTour && e.kind == League.Kind.ClubChampion));
        Assert.AreEqual(9, league.Count(e => e.onTour && e.kind == League.Kind.Foreign));
    }

    [Test]
    public void NamesFitTheirCountry()
    {
        var all = League.Characters.Concat(League.Surnames).ToList();
        foreach (var home in League.Countries)
            foreach (var e in Build(home, 11).Skip(1))
                Assert.IsTrue(all.Any(n => n.name == e.name && n.From(e.country)), e.name + " in " + e.country);
    }

    [Test]
    public void ClubmatesAreCharactersFirst()
    {
        var mates = Build("CAN", 5).Where(e => e.kind == League.Kind.Clubmate).ToList();
        Assert.IsTrue(mates.All(e => e.character));
        var chinese = Build("CHN", 5).Where(e => e.kind == League.Kind.Clubmate).ToList();
        Assert.AreEqual(1, chinese.Count(e => e.character));
    }

    [Test]
    public void ThePlayersTeamNameIsNotRepeated()
    {
        for (int seed = 0; seed < 40; seed++)
            Assert.AreEqual(1, Build("ITA", seed, "tortellini").Count(e => e.name.ToLowerInvariant() == "tortellini"));
    }

    [Test]
    public void UnknownCountryAndBlankClubFallBack()
    {
        var random = new System.Random(1);
        var league = League.Build("Tester", null, "  ", random.Next);
        Assert.AreEqual("CAN", league[0].country);
        CollectionAssert.Contains(League.Towns("CAN"), league[0].club);
    }
}
