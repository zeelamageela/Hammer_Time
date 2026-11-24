using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// CareerManager state -> CareerSaveData -> JSON file -> CareerSaveData -> a second CareerManager, comparing every
/// field. The managers are created on an inactive GameObject so Awake (DontDestroyOnLoad, scene lookups) never runs;
/// ToSaveData/LoadFromSaveData are private, so they are called by reflection.
/// </summary>
public class CareerRoundTripTests
{
    const BindingFlags Any = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    string dir;
    string previousOverride;
    readonly List<GameObject> made = new List<GameObject>();

    [SetUp]
    public void SetUp()
    {
        previousOverride = CareerSaveService.DirectoryOverride;
        dir = Path.Combine(Path.GetTempPath(), "HammerTimeRoundTrip_" + Guid.NewGuid().ToString("N"));
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

    CareerManager NewManager()
    {
        var go = new GameObject("RoundTripCM");
        go.SetActive(false);                 // keep Awake from running
        made.Add(go);
        var cm = go.AddComponent<CareerManager>();
        // Not created by AddComponent (the Inspector normally creates them); the real Awake/scene data does.
        cm.cStats = new CareerStats();
        cm.oppStats = new CareerStats();
        cm.modStats = new CareerStats();
        return cm;
    }

    static void Set(object target, string field, object value)
    {
        var f = target.GetType().GetField(field, Any);
        Assert.NotNull(f, "CareerManager no longer has a field called " + field + " - update the round-trip test");
        f.SetValue(target, value);
    }

    static object Get(object target, string field)
    {
        var f = target.GetType().GetField(field, Any);
        Assert.NotNull(f, "CareerManager no longer has a field called " + field);
        return f.GetValue(target);
    }

    static Player MakePlayer(int seed)
    {
        return new Player
        {
            id = seed, name = "Player" + seed, description = "desc" + seed, cost = 100 + seed,
            weight = 10 + seed, finesse = 20 + seed, aim = 30 + seed,
            sweepStrength = 40 + seed, sweepEnduro = 50 + seed, sweepCohesion = 60 + seed,
            oppDraw = 11 + seed, oppGuard = 12 + seed, oppTakeOut = 13 + seed,
            oppStrength = 14 + seed, oppEnduro = 15 + seed, oppCohesion = 16 + seed,
        };
    }

    static Team MakeTeam(int id, bool isPlayer)
    {
        var t = new Team
        {
            id = id, name = "Team" + id, player = isPlayer,
            tournamentWins = 3 + id, tournamentLosses = 4 + id, tournamentEarnings = 1234.5f + id,
            rank = 5 + id, nextOpp = "Opp" + id, poolId = 1, pointDifferential = -7 - id,
            seasonWins = 8 + id, seasonLosses = 9 + id, seasonEarnings = 4321.5f + id,
            tourPoints = 77.5f + id,
            weight = 21 + id, aim = 22 + id, finesse = 23 + id,
            sweepStrength = 24 + id, sweepEnduro = 25 + id, sweepCohesion = 26 + id,
            tourRecord = new Vector2(2 + id, 3 + id),
        };
        t.players = new List<Player> { MakePlayer(id * 10 + 1), MakePlayer(id * 10 + 2) };
        return t;
    }

    void FillCareer(CareerManager cm)
    {
        Set(cm, "playerName", "Greezy");
        Set(cm, "teamName", "Rock Stars");
        Set(cm, "teamColour", new Color(0.25f, 0.5f, 0.75f, 1f));
        Set(cm, "playerTeamIndex", 3);
        Set(cm, "week", 7);
        Set(cm, "season", 2);
        Set(cm, "cash", 4321.5f);
        Set(cm, "earnings", 98765.25f);
        Set(cm, "xp", 135f);
        Set(cm, "level", 4);
        Set(cm, "skillPoints", 33);
        Set(cm, "record", new Vector2(14, 6));
        Set(cm, "provQual", true);
        Set(cm, "tourQual", true);

        var c = (CareerStats)Get(cm, "cStats");
        c.weightAccuracy = 41; c.finesseAccuracy = 42; c.aimAccuracy = 43;
        c.sweepStrength = 44; c.sweepEndurance = 45; c.sweepCohesion = 46;
        var o = (CareerStats)Get(cm, "oppStats");
        o.weightAccuracy = 51; o.finesseAccuracy = 52; o.aimAccuracy = 53;
        o.sweepStrength = 54; o.sweepEndurance = 55; o.sweepCohesion = 56;

        Set(cm, "teams", new[] { MakeTeam(0, true), MakeTeam(1, false), MakeTeam(2, false) });
        Set(cm, "tourTeams", new[] { MakeTeam(1, false), MakeTeam(2, false) });
        Set(cm, "teamRecords", new[] { new Vector4(5, 6, 700.5f, 0), new Vector4(7, 8, 900.5f, 1), new Vector4(1, 2, 3.5f, 2) });
        Set(cm, "tourRecords", new[] { new Vector4(2, 1, 50.5f, 1), new Vector4(0, 3, 5.5f, 2) });
        Set(cm, "activePlayers", new[] { MakePlayer(1), MakePlayer(2), MakePlayer(3), MakePlayer(4) });

        Set(cm, "activeEquipID", new[] { 5, 31, 62, 93 });
        Set(cm, "inventoryID", new[] { 0, 5, 30, 31, 60, 62, 90, 93 });
        Set(cm, "activeHandleTier", 3);
        Set(cm, "trainingLevels", new[] { 1, 0, 3, 2, 4 });

        Set(cm, "cardPUIDList", new[] { 1, 2, 3 });
        Set(cm, "cardSponsorIDList", new[] { 4, 5 });
        Set(cm, "activeCardIDList", new[] { 6 });
        Set(cm, "playedCardIDList", new[] { 7, 8 });
        Set(cm, "activeCardLengthList", new[] { 2, 3 });

        Set(cm, "tournyResults", new List<int> { 1, 3, 2 });
        Set(cm, "allTimeTrophyList", new List<bool> { true, false, true });
        Set(cm, "currentTrophyList", new List<bool> { false, true });

        Set(cm, "coachDialogue", new[] { true, false });
        Set(cm, "qualDialogue", new[] { false, true });
        Set(cm, "storyBlock", 4);
    }

    // Everything the round trip is expected to preserve on CareerManager itself.
    static readonly string[] CareerFields =
    {
        "playerName", "teamName", "teamColour", "playerTeamIndex", "week", "season", "cash", "earnings", "xp", "level",
        "skillPoints", "record", "provQual", "tourQual", "cStats", "oppStats", "teams", "tourTeams", "teamRecords",
        "tourRecords", "activePlayers", "activeEquipID", "inventoryID", "activeHandleTier", "trainingLevels", "cardPUIDList",
        "cardSponsorIDList", "activeCardIDList", "playedCardIDList", "activeCardLengthList", "tournyResults",
        "allTimeTrophyList", "currentTrophyList", "coachDialogue", "qualDialogue", "storyBlock",
    };

    // Runtime-only fields on Team/Player that are not meant to be saved.
    static readonly HashSet<string> RuntimeOnly = new HashSet<string> { "image", "active", "view", "record" };

    static string Describe(object v)
    {
        if (v == null) return "null";
        if (v is string s) return "\"" + s + "\"";
        if (v is Color c) return $"rgba({c.r:0.###},{c.g:0.###},{c.b:0.###},{c.a:0.###})";
        if (v is Vector2 v2) return $"({v2.x:0.###},{v2.y:0.###})";
        if (v is Vector4 v4) return $"({v4.x:0.###},{v4.y:0.###},{v4.z:0.###},{v4.w:0.###})";
        if (v is IEnumerable e && !(v is string))
            return "[" + string.Join(", ", e.Cast<object>().Select(Describe)) + "]";
        var t = v.GetType();
        if (t.IsPrimitive || t.IsEnum) return Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture);
        if (t.IsClass || (t.IsValueType && !t.IsPrimitive))
        {
            var sb = new StringBuilder("{");
            foreach (var f in t.GetFields(BindingFlags.Instance | BindingFlags.Public).OrderBy(f => f.Name))
            {
                if (RuntimeOnly.Contains(f.Name)) continue;
                sb.Append(f.Name).Append('=').Append(Describe(f.GetValue(v))).Append("; ");
            }
            return sb.Append('}').ToString();
        }
        return v.ToString();
    }

    List<string> Compare(CareerManager before, CareerManager after)
    {
        var lost = new List<string>();
        foreach (var name in CareerFields)
        {
            string a = Describe(Get(before, name));
            string b = Describe(Get(after, name));
            if (a != b) lost.Add($"{name}\n      before: {a}\n      after:  {b}");
        }
        return lost;
    }

    CareerSaveData ToData(CareerManager cm)
    {
        var m = typeof(CareerManager).GetMethod("ToSaveData", Any);
        Assert.NotNull(m, "ToSaveData not found");
        return (CareerSaveData)m.Invoke(cm, null);
    }

    void LoadData(CareerManager cm, CareerSaveData data)
    {
        var m = typeof(CareerManager).GetMethod("LoadFromSaveData", Any);
        Assert.NotNull(m, "LoadFromSaveData not found");
        m.Invoke(cm, new object[] { data });
    }

    [Test]
    public void Comparison_ActuallyNoticesLostOrChangedFields()
    {
        // Guards the guard: the round-trip tests above are only worth anything if Compare can fail.
        var a = NewManager();
        FillCareer(a);
        var b = NewManager();
        LoadData(b, ToData(a));
        Assert.IsEmpty(Compare(a, b));

        b.week += 1;
        b.teams[1].players[0].sweepEnduro += 1;
        b.activePlayers[3].aim += 1;
        b.activeHandleTier = 0;

        var lost = Compare(a, b);
        Assert.AreEqual(4, lost.Count, "expected week, teams, activePlayers and activeHandleTier to differ, got:\n" + string.Join("\n", lost));
    }

    [Test]
    public void Career_SurvivesSaveToDiskAndLoad()
    {
        var before = NewManager();
        FillCareer(before);

        CareerSaveData data = ToData(before);
        Assert.IsTrue(CareerSaveService.SaveCareer(data), "SaveCareer refused the data (validation)");
        CareerSaveData loaded = CareerSaveService.LoadCareer();
        Assert.NotNull(loaded, "LoadCareer returned nothing");

        var after = NewManager();
        LoadData(after, loaded);

        var lost = Compare(before, after);
        Assert.IsEmpty(lost, "These fields did not survive a save and load:\n  - " + string.Join("\n  - ", lost));
    }

    [Test]
    public void Career_SurvivesAnInMemoryConversionToo()
    {
        // Same as above without the file, so a failure here is the field mapping and a pass-here-fail-there is JSON.
        var before = NewManager();
        FillCareer(before);

        var after = NewManager();
        LoadData(after, ToData(before));

        var lost = Compare(before, after);
        Assert.IsEmpty(lost, "These fields did not survive ToSaveData/LoadFromSaveData:\n  - " + string.Join("\n  - ", lost));
    }

    [Test]
    public void SaveFromAMatchScene_KeepsTheOwnedEquipmentCollections()
    {
        // The arena's EquipmentManager is gone in a match scene; saving there used to write an empty collection.
        var arena = new GameObject("TestEquipMgr");
        made.Add(arena);
        var em = arena.AddComponent<EquipmentManager>();
        em.handles = new[] { new Equipment { id = 0, cost = 0 }, new Equipment { id = 9, cost = 10830f } };
        em.heads = new[] { new Equipment { id = 30, cost = 0 } };
        em.footwear = new[] { new Equipment { id = 60, cost = 0 } };
        em.apparel = new[] { new Equipment { id = 80, cost = 0 } };

        var cm = NewManager();
        var inArena = ToData(cm);
        Assert.AreEqual(2, inArena.allEquipment.handles.Count);

        UnityEngine.Object.DestroyImmediate(arena);   // leave the arena
        var inMatch = ToData(cm);

        Assert.AreEqual(2, inMatch.allEquipment.handles.Count, "handles wiped by a save with no EquipmentManager");
        Assert.AreEqual(10830f, inMatch.allEquipment.handles[1].cost, 0.01f);
        Assert.AreEqual(1, inMatch.allEquipment.apparel.Count);
    }

    [Test]
    public void Career_SavedTwice_StaysIdentical()
    {
        // Load -> save -> load again must not drift (a field written differently from how it is read would creep).
        var first = NewManager();
        FillCareer(first);

        var second = NewManager();
        LoadData(second, ToData(first));
        var third = NewManager();
        LoadData(third, ToData(second));

        var drift = Compare(second, third);
        Assert.IsEmpty(drift, "Fields changed on a second save/load cycle:\n  - " + string.Join("\n  - ", drift));
    }
}
