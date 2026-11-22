/// <summary>
/// Clears every "where was the player in the game flow" flag when a new career starts. LoadToCM routes by these flags,
/// so any flag left over from a loaded save (e.g. inEndMenu) sends the new career into the old career's game.
/// </summary>
public static class NewCareerState
{
    public static void ResetGameFlow(GameSettingsPersist gsp)
    {
        gsp.careerLoad = false;
        gsp.loadGame = false;
        gsp.gameInProgress = false;
        gsp.tournyInProgress = false;
        gsp.inEndMenu = false;
        gsp.justFinishedGame = false;
        gsp.tournyIntroShown = false;
        gsp.draw = 0;
        gsp.playoffRound = 0;
        gsp.rockCurrent = 0;
        gsp.endCurrent = 0;
        gsp.redScore = 0;
        gsp.yellowScore = 0;
        gsp.rockPos = null;
        gsp.rockInPlay = null;
        gsp.score = null;
    }
}
