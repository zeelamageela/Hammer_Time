using NUnit.Framework;
using UnityEngine;

public class NewCareerStateTests
{
    GameObject go;
    GameSettingsPersist gsp;

    [SetUp]
    public void SetUp()
    {
        go = new GameObject("gsp");
        gsp = go.AddComponent<GameSettingsPersist>();
    }

    [TearDown]
    public void TearDown() => Object.DestroyImmediate(go);

    [Test]
    public void ResetGameFlow_ClearsEverythingLoadToCMRoutesOn()
    {
        // The state a loaded save leaves behind: finished a game at week 4, viewing its end menu, mid-tournament.
        gsp.gameInProgress = true;
        gsp.tournyInProgress = true;
        gsp.inEndMenu = true;
        gsp.justFinishedGame = true;
        gsp.tournyIntroShown = true;
        gsp.loadGame = true;
        gsp.careerLoad = true;
        gsp.draw = 3;
        gsp.playoffRound = 3;

        NewCareerState.ResetGameFlow(gsp);

        Assert.IsFalse(gsp.gameInProgress);
        Assert.IsFalse(gsp.tournyInProgress);
        Assert.IsFalse(gsp.inEndMenu, "left set, a new career is routed to the old career's end menu");
        Assert.IsFalse(gsp.justFinishedGame);
        Assert.IsFalse(gsp.tournyIntroShown);
        Assert.IsFalse(gsp.loadGame);
        Assert.IsFalse(gsp.careerLoad);
        Assert.AreEqual(0, gsp.draw);
        Assert.AreEqual(0, gsp.playoffRound);
    }
}
