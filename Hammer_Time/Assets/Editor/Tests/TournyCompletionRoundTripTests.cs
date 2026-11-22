using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Which tournaments are complete / won must survive save -> file -> load, both when a TournySelector exists at save
/// time (the hub) and when it doesn't (saving from inside a match). TournySelector has no Awake/OnEnable, so it can be
/// added to an active GameObject in EditMode without running any of its logic.
/// </summary>
public class TournyCompletionRoundTripTests
{
    const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    string dir;
    string previousOverride;
    readonly List<GameObject> made = new List<GameObject>();

    [SetUp]
    public void SetUp()
    {
        previousOverride = CareerSaveService.DirectoryOverride;
        dir = Path.Combine(Path.GetTempPath(), "HammerTimeTournyRT_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        CareerSaveService.DirectoryOverride = dir;
    }

    [TearDown]
    public void TearDown()
    {
        CareerSaveService.DirectoryOverride = previousOverride;
        foreach (var go in made) if (go != null) UnityEngine.Object.DestroyImmediate(go);
        made.Clear();
        try { Directory.Delete(dir, true); } catch { }
    }

    static Tourny[] Make(int firstId, int count)
    {
        var a = new Tourny[count];
        for (int i = 0; i < count; i++) a[i] = new Tourny { id = firstId + i, name = "T" + (firstId + i) };
        return a;
    }

    TournySelector NewSelector(bool active = true)
    {
        var go = new GameObject("RoundTripTSel");
        made.Add(go);
        var t = go.AddComponent<TournySelector>();
        t.tournies = Make(1, 4);
        t.tour = Make(10, 3);
        t.provQual = Make(50, 3);
        t.tourChampionship = new Tourny { id = 100, name = "Tour Champ", championship = true };
        t.provChampionship = new Tourny { id = 101, name = "Prov Champ", championship = true };
        go.SetActive(active);
        return t;
    }

    CareerManager NewManager()
    {
        var go = new GameObject("RoundTripCM");
        go.SetActive(false);
        made.Add(go);
        var cm = go.AddComponent<CareerManager>();
        cm.cStats = new CareerStats();
        cm.oppStats = new CareerStats();
        cm.modStats = new CareerStats();
        cm.playerName = "Greezy";
        cm.teamName = "Rock Stars";
        cm.teams = new[] { new Team { id = 0, name = "Rock Stars", player = true } };
        cm.week = 5;
        return cm;
    }

    static void ApplyPattern(TournySelector t)
    {
        t.tournies[0].complete = true;
        t.tournies[2].complete = true; t.tournies[2].trophyWon = true;
        t.tour[1].complete = true; t.tour[1].trophyWon = true;
        t.provQual[0].complete = true;
        t.provQual[2].complete = true; t.provQual[2].trophyWon = true;
        t.tourChampionship.complete = true; t.tourChampionship.trophyWon = true;   // won the Tour title
        t.provChampionship.complete = true;                                        // played the Provincials, lost
    }

    static string Describe(TournySelector t)
    {
        string One(Tourny x) => $"{x.id}:{(x.complete ? "C" : "-")}{(x.trophyWon ? "W" : "-")}";
        var all = t.tournies.Concat(t.tour).Concat(t.provQual).Concat(new[] { t.tourChampionship, t.provChampionship });
        return string.Join(" ", all.Select(One));
    }

    CareerSaveData Through(CareerManager cm)
    {
        var toData = typeof(CareerManager).GetMethod("ToSaveData", Any);
        var data = (CareerSaveData)toData.Invoke(cm, null);
        Assert.IsTrue(CareerSaveService.SaveCareer(data), "SaveCareer refused the data");
        var loaded = CareerSaveService.LoadCareer();
        Assert.NotNull(loaded);
        return loaded;
    }

    static void Apply(CareerManager cm, TournySelector into, CareerSaveData data)
    {
        typeof(CareerManager).GetMethod("ApplyTournamentData", Any).Invoke(cm, new object[] { into, data });
    }

    [Test]
    public void Completion_SavedFromTheHub_IsRestored()
    {
        var before = NewSelector();
        ApplyPattern(before);

        CareerSaveData loaded = Through(NewManager());   // finds `before` through FindFirstObjectByType

        var after = NewSelector(active: false);
        Apply(NewManager(), after, loaded);

        Assert.AreEqual(Describe(before), Describe(after),
            "C = complete, W = trophy won, id:flags in order tournies, tour, provQual, tour champ, prov champ");
    }

    [Test]
    public void Completion_SavedFromInsideAMatch_IsRestored()
    {
        // No TournySelector in the match scene: the manager falls back to its own copies of the arrays.
        var source = NewSelector(active: false);
        ApplyPattern(source);

        var cm = NewManager();
        cm.tournies = source.tournies;
        cm.tour = source.tour;
        cm.prov = source.provQual;
        cm.champ = new[] { source.tourChampionship, source.provChampionship };

        CareerSaveData loaded = Through(cm);

        var after = NewSelector(active: false);
        Apply(NewManager(), after, loaded);

        Assert.AreEqual(Describe(source), Describe(after));
    }

    [Test]
    public void Completion_AcrossTwoSaves_DoesNotLoseEarlierResults()
    {
        // Win one thing, save, load, win another, save again: the first result must still be there.
        var first = NewSelector();
        first.tour[0].complete = true;
        first.tour[0].trophyWon = true;
        first.tourChampionship.complete = true;
        first.tourChampionship.trophyWon = true;
        var firstSave = Through(NewManager());

        // Reload into a fresh hub (the old one goes away, as it does on a scene change), then play on.
        UnityEngine.Object.DestroyImmediate(first.gameObject);
        var second = NewSelector();
        Apply(NewManager(), second, firstSave);
        second.tour[1].complete = true;
        var secondSave = Through(NewManager());

        UnityEngine.Object.DestroyImmediate(second.gameObject);
        var final = NewSelector(active: false);
        Apply(NewManager(), final, secondSave);

        Assert.IsTrue(final.tour[0].complete && final.tour[0].trophyWon, "first result lost");
        Assert.IsTrue(final.tour[1].complete, "second result lost");
        Assert.IsTrue(final.tourChampionship.complete && final.tourChampionship.trophyWon, "championship trophy lost");
    }
}
