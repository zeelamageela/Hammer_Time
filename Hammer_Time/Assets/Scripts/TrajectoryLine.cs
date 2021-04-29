using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrajectoryLine : MonoBehaviour
{
    private LineRenderer lr;
    public GameObject launcher;
    public GameManager gm;

    GameObject rock;



    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.material = new Material(Shader.Find("Sprites/Default"));

        rock = gm.rockList[gm.rockCurrent].rock;


    }

    private void Update()
    {
        if (rock != null)
        {
            StartCoroutine(PullLine());
        }
    }

    IEnumerator PullLine()
    {
        Debug.Log("Drawing line");
        Vector3[] positions = new Vector3[2];
        positions[0] = new Vector3(launcher.transform.position.x, launcher.transform.position.y, 0f);
        positions[1] = new Vector3(rock.transform.position.x, rock.transform.position.y, 0f);

        lr.positionCount = positions.Length;
        lr.SetPositions(positions);

        yield return new WaitForFixedUpdate();
    }
}