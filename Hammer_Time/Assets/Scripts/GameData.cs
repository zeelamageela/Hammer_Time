using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameData
{
    public int endCurrent;
    public int endTotal;
    public int redScore;
    public int yellowScore;
    public bool aiRed;
    public bool aiYellow;
    public bool redHammer;

    public int rockCurrent;
    public int rockTotal;

    public float[] rockPosX;
    public float[] rockPosY;
    public bool[] inPlay;

    public GameData (GameManager gm)
    {
        endCurrent = gm.endCurrent;
        endTotal = gm.endTotal;
        redScore = gm.redScore;
        yellowScore = gm.yellowScore;
        aiRed = gm.aiTeamRed;
        aiYellow = gm.aiTeamYellow;
        redHammer = gm.redHammer;

        rockCurrent = gm.rockCurrent;
        rockTotal = gm.rockTotal;

        rockPosX = new float[rockTotal];
        rockPosY = new float[rockTotal];

        inPlay = new bool[rockTotal];

        for (int i = 0; i < rockCurrent; i++)
        {
            inPlay[i] = gm.rockList[i].rockInfo.inPlay;
            rockPosX[i] = gm.rockList[i].rock.transform.position.x;
            rockPosY[i] = gm.rockList[i].rock.transform.position.y;

        }
    }
}
