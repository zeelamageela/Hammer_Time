using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public GameManager gm;
    public SweeperManager sm;
    public GameObject[] teamRed;
    public GameObject[] teamYellow;

    public GameObject[] leadGO;
    public GameObject[] secondGO;
    public GameObject[] thirdGO;
    public GameObject[] skipGO;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (gm.rockList.Count != 0)
        {
            #region Target
            //if (gm.rockCurrent <= 11)
            //{
            //    if (gm.redHammer && gm.rockList[gm.rockCurrent].rockInfo.teamName == gm.rockList[1].rockInfo.teamName)
            //    {
            //        if (!gm.aiTeamRed)
            //            gm.target = true;
            //        else
            //            gm.target = false;
            //    }
            //    else if (gm.redHammer && gm.rockList[gm.rockCurrent].rockInfo.teamName == gm.rockList[0].rockInfo.teamName)
            //    {
            //        if (!gm.aiTeamYellow)
            //            gm.target = true;
            //        else
            //            gm.target = false;
            //    }
            //    else if (!gm.redHammer && gm.rockList[gm.rockCurrent].rockInfo.teamName == gm.rockList[0].rockInfo.teamName)
            //    {
            //        if (!gm.aiTeamRed)
            //            gm.target = true;
            //        else
            //            gm.target = false;
            //    }
            //    else if (!gm.redHammer && gm.rockList[gm.rockCurrent].rockInfo.teamName == gm.rockList[1].rockInfo.teamName)
            //    {
            //        if (!gm.aiTeamYellow)
            //            gm.target = true;
            //        else
            //            gm.target = false;
            //    }
            //    else
            //        gm.target = false;
            //}
            //else
            //    gm.target = false;

            //if (gm.rockCurrent <= 3)
            //{
            //    gm.shooterAnimRed = leadGO[0];
            //    gm.shooterAnimYellow = leadGO[1];

            //}
            //else if (gm.rockCurrent <= 7)
            //{
            //    gm.shooterAnimRed = secondGO[0];
            //    gm.shooterAnimYellow = secondGO[1];
            //}
            //else if (gm.rockCurrent <= 11)
            //{
            //    gm.shooterAnimRed = thirdGO[0];
            //    gm.shooterAnimYellow = thirdGO[1];
            //}
            //else
            //{
            //    gm.shooterAnimRed = skipGO[0];
            //    gm.shooterAnimYellow = skipGO[1];
            //}
            #endregion
        }


    }

    public void SetCharacter(int rockCurrent, bool redTurn)
    {
        #region Target
        //if (!gm.gsp.story)
        //{
        //    if (gm.gsp.third)
        //    {
        //        if (rockCurrent <= 11 && rockCurrent >= 8)
        //        {
        //            gm.target = false;
        //        }
        //        else
        //        {
        //            if (redTurn)
        //            {
        //                if (!gm.aiTeamRed)
        //                    gm.target = true;
        //                else
        //                    gm.target = false;
        //            }
        //            else if (!redTurn)
        //            {
        //                if (!gm.aiTeamYellow)
        //                    gm.target = true;
        //                else
        //                    gm.target = false;
        //            }
        //            else
        //                gm.target = false;
        //        }
        //    }
        //    else if (gm.gsp.skip)
        //    {
        //        if (rockCurrent <= 11)
        //        {
        //            if (redTurn)
        //            {
        //                if (!gm.aiTeamRed)
        //                    gm.target = true;
        //                else
        //                    gm.target = false;
        //            }
        //            else if (!redTurn)
        //            {
        //                if (!gm.aiTeamYellow)
        //                    gm.target = true;
        //                else
        //                    gm.target = false;
        //            }
        //            else
        //                gm.target = false;
        //        }
        //        else
        //            gm.target = false;
        //    }
        //    else
        //        gm.target = false;

        //}
        //else 
        //{
        //    if (gm.gsp.third)
        //    {
        //        if (rockCurrent <= 11)
        //        {
        //            if (rockCurrent >= 8)
        //            {
        //                if (redTurn)
        //                {
        //                    if (!gm.aiTeamRed)
        //                        gm.target = true;
        //                    else
        //                        gm.target = false;
        //                }
        //                else if (!redTurn)
        //                {
        //                    if (!gm.aiTeamYellow)
        //                        gm.target = true;
        //                    else
        //                        gm.target = false;
        //                }
        //                else
        //                    gm.target = false;
        //            }
        //            else
        //                gm.target = false;
        //        }
        //        else
        //            gm.target = false;
        //    }
        //    else if (gm.gsp.skip)
        //    {
        //        if (rockCurrent <= 11)
        //        {
        //            if (redTurn)
        //            {
        //                if (!gm.aiTeamRed)
        //                    gm.target = true;
        //                else
        //                    gm.target = false;
        //            }
        //            else if (!redTurn)
        //            {
        //                if (!gm.aiTeamYellow)
        //                    gm.target = true;
        //                else
        //                    gm.target = false;
        //            }
        //            else
        //                gm.target = false;
        //        }
        //        else
        //            gm.target = false;
        //    }
        //    else
        //        gm.target = false;
        //}
        #endregion

        if (gm.rockCurrent <= 3)
        {
            gm.shooterAnimRed = leadGO[0];
            gm.shooterAnimYellow = leadGO[1];
        }
        else if (gm.rockCurrent <= 7)
        {
            gm.shooterAnimRed = secondGO[0];
            gm.shooterAnimYellow = secondGO[1];
        }
        else if (gm.rockCurrent <= 11)
        {
            gm.shooterAnimRed = thirdGO[0];
            gm.shooterAnimYellow = thirdGO[1];
        }
        else
        {
            gm.shooterAnimRed = skipGO[0];
            gm.shooterAnimYellow = skipGO[1];
        }
    }
}
