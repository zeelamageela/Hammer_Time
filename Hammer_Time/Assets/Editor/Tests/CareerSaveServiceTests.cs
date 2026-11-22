using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CareerSaveServiceTests
{
    string dir;

    [SetUp]
    public void SetUp()
    {
        dir = Path.Combine(Path.GetTempPath(), "ht_save_tests_" + System.Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        CareerSaveService.DirectoryOverride = dir;
        // The service logs errors on purpose when handling bad files.
        LogAssert.ignoreFailingMessages = true;
    }

    [TearDown]
    public void TearDown()
    {
        CareerSaveService.DirectoryOverride = null;
        LogAssert.ignoreFailingMessages = false;
        if (Directory.Exists(dir)) Directory.Delete(dir, true);
    }

    static CareerSaveData MakeData(string name)
    {
        var d = new CareerSaveData
        {
            playerName = name,
            teamName = "Team " + name,
            teamColour = new SerializableColor(new Color(0.2f, 0.4f, 0.6f, 1f)),
            week = 7,
            season = 2,
            cash = 12345.5f,
            xp = 99f,
            level = 4,
            skillPoints = 3,
        };
        d.teams.Add(new TeamData { id = 1, name = "A", isPlayer = true, seasonWins = 5, tourPoints = 2.5f });
        d.activeEquipIDs.AddRange(new[] { 0, 3, 5, 8 });
        d.inventoryIDs.AddRange(new[] { 0, 3, 5 });
        d.completedTournamentIDs.AddRange(new[] { 1, 2, 9 });
        d.trophyWonIDs.Add(2);
        d.dialogueFlags = new DialogueFlagsData { coachDialogue = new[] { true, false, true }, storyBlockIndex = 6 };
        d.currentGameState = new GameStateData
        {
            gameInProgress = true, tournyInProgress = true, ends = 8, rocks = 8,
            currentEnd = 3, currentRock = 11, redScore = 4, yellowScore = 2,
            redTeamName = "Red", yellowTeamName = "Yellow",
        };
        d.currentGameState.endScores.Add(new Vector2IntData(new Vector2Int(1, 0)));
        d.currentGameState.endScores.Add(new Vector2IntData(new Vector2Int(0, 2)));
        d.currentGameState.rockPositions.Add(new Vector2Data(new Vector2(0.5f, 6.25f)));
        d.currentGameState.rockInPlay.Add(true);
        return d;
    }

    static string Backup(string d) => Path.Combine(d, "career_save_backup.json");
    static string Main(string d) => Path.Combine(d, "career_save.json");

    [Test]
    public void RoundTrip_PreservesCareerAndGameState()
    {
        Assert.IsTrue(CareerSaveService.SaveCareer(MakeData("Zack")));

        var l = CareerSaveService.LoadCareer();

        Assert.NotNull(l);
        Assert.AreEqual("Zack", l.playerName);
        Assert.AreEqual(7, l.week);
        Assert.AreEqual(12345.5f, l.cash);
        Assert.AreEqual(new[] { 0, 3, 5 }, l.inventoryIDs.ToArray());
        Assert.AreEqual(new[] { 1, 2, 9 }, l.completedTournamentIDs.ToArray());
        Assert.AreEqual(6, l.dialogueFlags.storyBlockIndex);
        Assert.IsTrue(l.currentGameState.gameInProgress);
        Assert.AreEqual(11, l.currentGameState.currentRock);
        Assert.AreEqual(2, l.currentGameState.endScores.Count);
        Assert.AreEqual(2, l.currentGameState.endScores[1].y);
        Assert.AreEqual(6.25f, l.currentGameState.rockPositions[0].y);
        Assert.AreEqual(0.4f, l.teamColour.g, 0.0001f);
    }

    [Test]
    public void InvalidData_IsRefused_AndExistingSaveUntouched()
    {
        LogAssert.ignoreFailingMessages = true;
        CareerSaveService.SaveCareer(MakeData("Good"));
        string before = File.ReadAllText(Main(dir));

        var blank = MakeData("");          // what a pause/quit save fires before a career exists
        bool ok = CareerSaveService.SaveCareer(blank);

        Assert.IsFalse(ok, "a save that would fail validation on load must not be written");
        Assert.AreEqual(before, File.ReadAllText(Main(dir)));
    }

    [Test]
    public void CorruptMainFile_DoesNotOverwriteGoodBackup()
    {
        LogAssert.ignoreFailingMessages = true;
        CareerSaveService.SaveCareer(MakeData("First"));
        CareerSaveService.SaveCareer(MakeData("Second"));   // backup now holds First
        File.WriteAllText(Main(dir), "{ truncated");        // crash mid-write

        CareerSaveService.SaveCareer(MakeData("Third"));

        var backup = JsonUtility.FromJson<CareerSaveData>(File.ReadAllText(Backup(dir)));
        Assert.IsNotNull(backup);
        Assert.AreEqual("First", backup.playerName, "corrupt file must not be rotated into the backup slot");
    }

    [Test]
    public void CorruptMainFile_LoadFallsBackToBackup()
    {
        LogAssert.ignoreFailingMessages = true;
        CareerSaveService.SaveCareer(MakeData("First"));
        CareerSaveService.SaveCareer(MakeData("Second"));
        File.WriteAllText(Main(dir), "{ truncated");

        var l = CareerSaveService.LoadCareer();

        Assert.NotNull(l);
        Assert.AreEqual("First", l.playerName);
    }

    [Test]
    public void Save_LeavesNoTempFileBehind_AndIgnoresStaleOne()
    {
        File.WriteAllText(Main(dir) + ".tmp", "garbage from an interrupted write");

        Assert.IsTrue(CareerSaveService.SaveCareer(MakeData("Zack")));

        Assert.AreEqual(0, Directory.GetFiles(dir, "*.tmp").Length);
        Assert.AreEqual("Zack", CareerSaveService.LoadCareer().playerName);
    }

    [Test]
    public void SaveOverExisting_ReplacesContents()
    {
        CareerSaveService.SaveCareer(MakeData("One"));
        CareerSaveService.SaveCareer(MakeData("Two"));

        Assert.AreEqual("Two", CareerSaveService.LoadCareer().playerName);
        Assert.AreEqual("One", JsonUtility.FromJson<CareerSaveData>(File.ReadAllText(Backup(dir))).playerName);
    }

    [Test]
    public void FailedLoad_PreservesUnreadableFileForRecovery()
    {
        LogAssert.ignoreFailingMessages = true;
        const string garbage = "{ not a valid save";
        File.WriteAllText(Main(dir), garbage);

        Assert.IsNull(CareerSaveService.LoadCareer());

        string kept = Path.Combine(dir, "career_save_failed_load.json");
        Assert.IsTrue(File.Exists(kept), "unreadable save should be copied aside before anything can overwrite it");
        Assert.AreEqual(garbage, File.ReadAllText(kept));
    }

    static string Replaced(string d) => Path.Combine(d, "career_save_replaced.json");

    [Test]
    public void NewCareer_KeepsThePreviousCareerRecoverable()
    {
        CareerSaveService.SaveCareer(MakeData("Old"));
        CareerSaveService.SaveCareer(MakeData("Old"));     // second save creates the backup
        string before = File.ReadAllText(Main(dir));

        Assert.IsTrue(CareerSaveService.ArchiveSave());
        CareerSaveService.DeleteSave();                    // what CareerManager.DeleteCareerSave does

        Assert.IsFalse(CareerSaveService.SaveExists());
        Assert.IsFalse(File.Exists(Backup(dir)));
        Assert.AreEqual(before, File.ReadAllText(Replaced(dir)), "the replaced career must be an exact copy");
    }

    [Test]
    public void ArchivedCareer_CanBeRestoredAndLoaded()
    {
        CareerSaveService.SaveCareer(MakeData("Old"));
        CareerSaveService.ArchiveSave();
        CareerSaveService.DeleteSave();

        File.Copy(Replaced(dir), Main(dir));
        var l = CareerSaveService.LoadCareer();

        Assert.NotNull(l);
        Assert.AreEqual("Old", l.playerName);
        Assert.AreEqual(7, l.week);
    }

    [Test]
    public void Archive_FallsBackToBackup_WhenMainSaveIsCorrupt_AndDoesNothingWithoutASave()
    {
        Assert.IsFalse(CareerSaveService.ArchiveSave(), "nothing to keep");
        Assert.IsFalse(File.Exists(Replaced(dir)));

        CareerSaveService.SaveCareer(MakeData("Good"));
        CareerSaveService.SaveCareer(MakeData("Good"));
        File.WriteAllText(Main(dir), "{ corrupt");

        Assert.IsTrue(CareerSaveService.ArchiveSave());
        Assert.AreEqual(File.ReadAllText(Backup(dir)), File.ReadAllText(Replaced(dir)));
    }
}
