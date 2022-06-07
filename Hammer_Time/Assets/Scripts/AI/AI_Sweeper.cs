using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Feedbacks;

public class AI_Sweeper : MonoBehaviour
{
    public AIManager aim;
    public GameManager gm;
    public SweeperManager sm;
    public MMFeedbackFloatingText fltText;

    //Transform target;
    
    public void OnSweep(bool aiTurn, string shotType, Vector2 target, bool inturn)
    {
        if (aiTurn)
            StartCoroutine(TargetShot(shotType, target, inturn));
        else
            StartCoroutine(PlayerSpeed(shotType, target, inturn));
    }
    IEnumerator TargetShot(string aiShotType, Vector2 target, bool inturn)
    {
        Rigidbody2D rockRB = gm.rockList[gm.rockCurrent].rock.GetComponent<Rigidbody2D>();
        GameObject rock = gm.rockList[gm.rockCurrent].rock;

        Debug.Log("Auto Sweep - " + aiShotType);

        switch (aiShotType)
        {
            #region Centre Guards
            case "Centre Guard":
                
                break;

            case "Tight Centre Guard":
                
                break;

            case "High Centre Guard":
                
                break;
            #endregion

            #region Corner Guards
            case "Left Corner Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.75f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.4f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 1.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.5f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                break;

            case "Left Tight Corner Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.25f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.75f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.5f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 1.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.25f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                break;

            case "Left High Corner Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.3f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.95f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.5f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                break;

            case "Right Corner Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.75f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.4f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 1.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.5f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                break;

            case "Right Tight Corner Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.25f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.75f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.5f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 1.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.25f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                break;

            case "Right High Corner Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.3f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.85f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.5f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                break;
            #endregion

            #region Twelve Foot Draws
            case "Top Twelve Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.58f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.04f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.85f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.48f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.46f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 1.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.25f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.44f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.42f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.5f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.34f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.32f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                sm.SweepWhoa(true);
                break;

            case "Left Twelve Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.9f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.45f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.3f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -1.62f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= -0.47f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.95f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -1.63f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= -0.74f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.52f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -1.55f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= -0.96f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                //yield return new WaitUntil(() => Mathf.Abs(rock.transform.position.x) >= 0.05f);
                //sm.SweepWhoa(true);

                break;

            case "Back Twelve Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 5.5f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.8f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.71f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.7f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.55f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.65f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.63f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.57f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.55f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 6.5f);
                Debug.Log("y = 6.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 6.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.5f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.44f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.41f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 7.75f);
                sm.SweepWhoa(true);
                break;

            case "Right Twelve Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.9f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.45f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.3f)
                    sm.SweepWeight(true);
                else if (!inturn && rock.transform.position.x <= 1.62f)
                    sm.SweepRight(true);
                else if (inturn && rock.transform.position.x >= 0.47f)
                    sm.SweepLeft(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.95f)
                    sm.SweepWeight(true);
                else if (!inturn && rock.transform.position.x <= 1.63f)
                    sm.SweepRight(true);
                else if (inturn && rock.transform.position.x >= 0.74f)
                    sm.SweepLeft(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.52f)
                    sm.SweepWeight(true);
                else if (!inturn && rock.transform.position.x <= 1.55f)
                    sm.SweepRight(true);
                else if (inturn && rock.transform.position.x >= 0.96f)
                    sm.SweepLeft(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 6.5f);
                sm.SweepWhoa(true);

                break;
            #endregion

            #region Four Foot Draws
            case "Button":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.9f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.45f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.3f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.65f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.65f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.95f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.55f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.55f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.52f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.4f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.4f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 6.5f);
                sm.SweepWhoa(true);

                break;

            case "Left Four Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.9f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.45f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.3f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -1f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.33f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.95f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.95f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.18f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.52f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.83f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => Mathf.Abs(0.37f - rock.transform.position.x) >= 0.1f);
                sm.SweepWhoa(true);

                break;

            case "Right Four Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.9f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.45f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.3f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.36f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 1f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.95f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.2f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.92f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.52f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.3f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.8f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);

                break;

            case "Top Four Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.75f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.3f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.2f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.65f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.65f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.85f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.5f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.5f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.4f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.35f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.35f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                break;

            case "Back Four Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 5f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.6f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.4f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.69f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.66f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.95f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.59f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.57f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.52f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.48f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= 0.45f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                //yield return new WaitUntil(() => Mathf.Abs(rock.transform.position.x) >= 0.05f);
                //sm.SweepWhoa(true);

                break;
            #endregion

            #region Take Outs
            case "Peel":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 8.35f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 7f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 5.75f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.45f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.9f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 7.75f);
                sm.SweepWhoa(true);
                break;

            case "Take Out":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 7.5f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 6f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.8f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.5f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.75f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 7.75f);
                sm.SweepWhoa(true);
                break;

            case "Tick":

                break;

            case "Raise":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 6f)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.65f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.4f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.45f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                sm.SweepWhoa(true);
                break;
            #endregion

            case "Draw To Target":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                float velLimit = ((5.5f - 4.58f) * ((target.y - 5.225f) / 2.55f)) + 4.58f;
                Debug.Log("y = -7 velLimit is " + velLimit);
                if (rockRB.velocity.y <= velLimit)
                    sm.SweepWeight(true);

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                velLimit = ((4f - 3f) * ((target.y - 5.225f) / 2.55f)) + 3f;
                if (rockRB.velocity.y <= velLimit)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                velLimit = ((2.8f - 1.85f) * ((target.y - 5.225f) / 2.55f)) + 1.85f;
                if (rockRB.velocity.y <= velLimit)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= target.x - 0.65f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= target.x + 0.65f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                velLimit = ((1.55f - 1.25f) * ((target.y - 5.225f) / 2.55f)) + 1.25f;
                if (rockRB.velocity.y <= velLimit)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= target.x - 0.55f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= target.x + 0.55f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                velLimit = ((1f - 0.5f) * ((target.y - 5.225f) / 2.55f)) + 0.5f;
                if (rockRB.velocity.y <= velLimit)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= target.x - 0.4f)
                    sm.SweepLeft(true);
                else if (!inturn && rock.transform.position.x >= target.x + 0.4f)
                    sm.SweepRight(true);
                else
                    sm.SweepWhoa(true);
                yield return new WaitUntil(() => rock.transform.position.y >= target.x);
                velLimit = 0.5f * ((target.y - 5.225f) / 2.55f);
                if (rockRB.velocity.y >= velLimit)
                    sm.SweepWhoa(true);

                break;

            case "Guard To Target":

                break;

            default:
                break;
        }

    }

    IEnumerator PlayerSpeed(string playerShotType, Vector2 target, bool inturn)
    {

        Rigidbody2D rockRB = gm.rockList[gm.rockCurrent].rock.GetComponent<Rigidbody2D>();
        GameObject rock = gm.rockList[gm.rockCurrent].rock;

        Debug.Log("sweeperL is " + sm.sweeperL.gameObject.activeSelf);
        fltText.TargetTransform = rock.transform;

        Debug.Log("Player Speed Callouts - " + playerShotType);

        float intensity = 1f;

        switch (playerShotType)
        {
            #region Centre Guards
            case "Centre Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.75f)
                    fltText.Value = "Rock is slow!!";

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);

                if (rockRB.velocity.y <= 2.4f)
                    fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1f)
                    fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 1.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.5f)
                    fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position, 2f);
                break;

            case "Tight Centre Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.25f)
                    fltText.Value = "Rock is slow!!";

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.75f)
                    fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.5f)
                    fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 1.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1f)
                    fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.25f)
                    fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                break;

            case "High Centre Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.3f)
                    fltText.Value = "Rock is slow!!";

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.95f)
                    fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.5f)
                    fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                break;
            #endregion

            #region Corner Guards
            case "Left Corner Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.75f)
                    fltText.Value = "Rock is slow!!";

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);

                if (rockRB.velocity.y <= 2.4f)
                    fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1f)
                    fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 1.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.5f)
                    fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position, 2f);
                break;

            case "Left Tight Corner Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.25f)
                    fltText.Value = "Rock is slow!!";

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.75f)
                    fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.5f)
                    fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 1.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1f)
                    fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.25f)
                    fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                break;

            case "Left High Corner Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.3f)
                    fltText.Value = "Rock is slow!!";

                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.95f)
                                         fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.5f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                break;

            case "Right Corner Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.75f)
                        fltText.Value = "Rock is slow!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.4f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 1.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.5f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                break;

            case "Right Tight Corner Guard":
                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.25f)
                        fltText.Value = "Rock is slow!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.75f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.5f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 1.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";
                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.25f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";
                break;

            case "Right High Corner Guard":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.3f)
                        fltText.Value = "Rock is slow!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.85f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.5f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                break;
            #endregion

            #region Twelve Foot Draws
            case "Top Twelve Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.58f)
                        fltText.Value = "Rock is slow!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.04f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.85f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.48f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.46f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";
                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 1.5f);
                Debug.Log("y = 1.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 1.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.25f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.44f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.42f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";
                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.5f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.34f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.32f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                fltText.Value = "Speed is good!!";
                break;

            case "Left Twelve Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.9f)
                        fltText.Value = "Rock is slow!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.45f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.3f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -1.62f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= -0.47f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";
                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.95f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -1.63f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= -0.74f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";
                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.52f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -1.55f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= -0.96f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";
                //yield return new WaitUntil(() => Mathf.Abs(rock.transform.position.x) >= 0.05f);
                //sm.SweepWhoa(true);

                break;

            case "Back Twelve Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 5.5f)
                        fltText.Value = "Rock is slow!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.8f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.71f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.7f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";
                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.55f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.65f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.63f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.57f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.55f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 6.5f);
                Debug.Log("y = 6.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 6.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.5f)
                    fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.44f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.41f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 7.75f);
                fltText.Value = "Speed is good!!";
                break;

            case "Right Twelve Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.9f)
                        fltText.Value = "Rock is slow!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.45f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.3f)
                        fltText.Value = "Rock is slow!!";
                else if (!inturn && rock.transform.position.x <= 1.62f)
                    fltText.Value = "Sweep the Curl!!";
                else if (inturn && rock.transform.position.x >= 0.47f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";
                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.95f)
                        fltText.Value = "Rock is slow!!";
                else if (!inturn && rock.transform.position.x <= 1.63f)
                    fltText.Value = "Sweep the Curl!!";
                else if (inturn && rock.transform.position.x >= 0.74f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";
                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.52f)
                        fltText.Value = "Rock is slow!!";
                else if (!inturn && rock.transform.position.x <= 1.55f)
                    fltText.Value = "Sweep the Curl!!";
                else if (inturn && rock.transform.position.x >= 0.96f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";
                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 6.5f);
                fltText.Value = "Speed is good!!";
                fltText.Play(rockRB.position);
                break;
            #endregion

            #region Four Foot Draws
            case "Button":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.9f)
                        fltText.Value = "Rock is slow!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.45f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.3f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.65f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.65f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.95f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.55f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.55f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.52f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.4f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.4f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 6.5f);
                fltText.Value = "Speed is good!!";

                break;

            case "Left Four Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.9f)
                        fltText.Value = "Rock is slow!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.45f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.3f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -1f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.33f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.95f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.95f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.18f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.52f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.83f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => Mathf.Abs(0.37f - rock.transform.position.x) >= 0.1f);
                fltText.Value = "Speed is good!!";
                fltText.Play(rockRB.position);
                break;

            case "Right Four Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.9f)
                        fltText.Value = "Rock is slow!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.45f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.3f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.36f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 1f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";
                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.95f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.2f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.92f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";
                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.52f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.3f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.8f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                break;

            case "Top Four Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 4.75f)
                        fltText.Value = "Rock is slow!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.3f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.2f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.65f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.65f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.85f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.5f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.5f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.4f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.35f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.35f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                break;

            case "Back Four Foot":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 5f)
                        fltText.Value = "Rock is slow!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 3.6f)
                        fltText.Value = "Rock is slow!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 2.4f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.69f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.66f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.95f)
                        fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.59f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.57f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";

                fltText.Play(rockRB.position);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 0.52f)
                    fltText.Value = "Rock is slow!!";
                else if (inturn && rock.transform.position.x <= -0.48f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= 0.45f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Speed is good!!";
                //yield return new WaitUntil(() => Mathf.Abs(rock.transform.position.x) >= 0.05f);
                //sm.SweepWhoa(true);
                fltText.Play(rockRB.position);
                break;
            #endregion


            case "Draw To Target":
                yield return new WaitUntil(() => rock.transform.position.y >= -7f);
                Debug.Log("y = -7 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -7 xPos is " + rock.transform.position.x);
                float velLimit = ((5.5f - 4.58f) * ((target.y - 5.225f) / 2.55f)) + 4.58f;
                Debug.Log("y = -7 velLimit is " + velLimit);
                if (rockRB.velocity.y <= velLimit)
                    fltText.Value = "Sweep!!";
                else
                    fltText.Value = "Leave it!!";
                fltText.Play(rockRB.position, 1.25f);
                yield return new WaitUntil(() => rock.transform.position.y >= -3.5f);
                Debug.Log("y = -3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = -3.5 xPos is " + rock.transform.position.x);
                velLimit = ((4f - 3f) * ((target.y - 5.225f) / 2.55f)) + 3f;
                if (rockRB.velocity.y <= velLimit)
                    fltText.Value = "SWEEP!!";
                else
                    fltText.Value = "Nope!!";

                fltText.Play(rockRB.position, 1.4f);
                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                velLimit = ((2.8f - 1.85f) * ((target.y - 5.225f) / 2.55f)) + 1.85f;
                if (rockRB.velocity.y <= velLimit)
                    fltText.Value = "SWEEEEEP!!";
                else if (inturn && rock.transform.position.x <= target.x - 0.65f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= target.x + 0.65f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "Leave it!!";

                fltText.Play(rockRB.position, 1.6f);
                yield return new WaitUntil(() => rock.transform.position.y >= 3.5f);
                Debug.Log("y = 3.5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 3.5 xPos is " + rock.transform.position.x);
                velLimit = ((1.55f - 1.25f) * ((target.y - 5.225f) / 2.55f)) + 1.25f;
                if (rockRB.velocity.y <= velLimit)
                    fltText.Value = "SWEEEEEEEEEP HARD!!";
                else if (inturn && rock.transform.position.x <= target.x - 0.55f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= target.x + 0.55f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "It's good!!";

                fltText.Play(rockRB.position, 1.75f);
                yield return new WaitUntil(() => rock.transform.position.y >= 5f);
                Debug.Log("y = 5 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 5 xPos is " + rock.transform.position.x);
                velLimit = ((1f - 0.5f) * ((target.y - 5.225f) / 2.55f)) + 0.5f;
                if (rockRB.velocity.y <= velLimit)
                    fltText.Value = "HARRRRRD!! GO GO GO!!!";
                else if (inturn && rock.transform.position.x <= target.x - 0.4f)
                    fltText.Value = "Sweep the Curl!!";
                else if (!inturn && rock.transform.position.x >= target.x + 0.4f)
                    fltText.Value = "Sweep the Curl!!";
                else
                    fltText.Value = "It's good!!";

                fltText.Play(rockRB.position, 1.85f);
                yield return new WaitUntil(() => rock.transform.position.y >= target.x);
                velLimit = 0.5f * ((target.y - 5.225f) / 2.55f);
                if (rockRB.velocity.y >= velLimit)
                    fltText.Value = "We're there!!";

                fltText.Play(rockRB.position, intensity);
                break;

            case "Guard To Target":

                break;

            default:
                break;
        }

        //fltText.Play(rockRB.position);
    }
}
