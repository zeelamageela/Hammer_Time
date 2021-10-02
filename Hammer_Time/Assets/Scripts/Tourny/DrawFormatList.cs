using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class DrawFormatList : MonoBehaviour
{
    public DrawFormat[] team5;
    public DrawFormat[] team6;
    public DrawFormat[] team7;
    public DrawFormat[] team8;
    public DrawFormat[] team9;
    public DrawFormat[] team10;
    public DrawFormat[] team11;
    public DrawFormat[] team12;
    public DrawFormat[] team13;
    public DrawFormat[] team14;
    public DrawFormat[] team15;
    public DrawFormat[] team16;

    public DrawFormat[] team16_2Pools;
    public DrawFormat[] team18_2Pools;
    public DrawFormat[] team20_2Pools;

    public DrawFormat[] currentFormat;

    public void DrawSelector(int numberOfTeams)
    {
        Debug.Log("Number of Teams is " + numberOfTeams);
        switch (numberOfTeams)
        {
            case 5:
                currentFormat = team5;
                break;

            case 6:
                currentFormat = team6;
                break;

            case 7:
                currentFormat = team7;
                break;

            case 8:
                currentFormat = team8;
                break;

            case 9:
                currentFormat = team9;
                break;

            case 10:
                currentFormat = team10;
                break;

            case 11:
                currentFormat = team11;
                break;

            case 12:
                currentFormat = team12;
                break;

            case 13:
                currentFormat = team13;
                break;

            case 14:
                currentFormat = team14;
                break;

            case 15:
                currentFormat = team15;
                break;

            case 16:
                currentFormat = team16;
                break;

            default:
                currentFormat = null;
                Debug.Log("Need between 5 and 16 teams for a tourny");
                break;
        }
    }
}

