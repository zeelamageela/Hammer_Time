using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_Sweeper : MonoBehaviour
{
    public AIManager aim;
    public GameManager gm;
    public SweeperManager sm;

    Transform target;

    public void OnSweep(string aiShotType, Vector2 target, bool inturn)
    {
        StartCoroutine(TargetShot(aiShotType, inturn));
    }
    IEnumerator TargetShot(string aiShotType, bool inturn)
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
                if (rockRB.velocity.y <= 3.1f)
                    sm.SweepWeight(true);
                else
                    sm.SweepWhoa(true);

                yield return new WaitUntil(() => rock.transform.position.y >= 0f);
                Debug.Log("y = 0 velocity is " + rockRB.velocity.x + ", " + rockRB.velocity.y);
                Debug.Log("y = 0 xPos is " + rock.transform.position.x);
                if (rockRB.velocity.y <= 1.85f)
                    sm.SweepWeight(true);
                else if (inturn && rock.transform.position.x <= -0.48f)
                    sm.SweepLeft(true);
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
                //yield return new WaitUntil(() => Mathf.Abs(rock.transform.position.x) >= 0.05f);
                //sm.SweepWhoa(true);

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

                break;

            case "Guard To Target":

                break;

            default:
                break;
        }

    }
}
