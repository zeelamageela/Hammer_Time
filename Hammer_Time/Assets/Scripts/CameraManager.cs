using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Cinemachine;

public class CameraManager : MonoBehaviour
{
    public Camera main;
    public Camera house;
    public Camera ui;
    public Camera top;
    public Camera persp;
    public Camera aim;

    public Transform aimMover;
    public Transform trajTarget;
    public Transform launcher;
    public GameManager gm;

    public GameObject vcam_go;
    public CinemachineVirtualCamera vcam;
    public CinemachineFramingTransposer vcam_ft;

    public HouseClick houseClick;

    private void Start()
    {
        vcam_ft = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();
        main.depth = 1;
        house.depth = -1;
        ui.depth = 3;
        top.depth = 2;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            TopView();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            HouseView();
        }

        vcam_ft.m_ScreenY = Mathf.Lerp(0.85f, 0.75f, (-10f - main.transform.position.y) / 6f);
    }
    public void ShotSetup()
    {
        vcam.enabled = false;
        vcam.m_Lens.OrthographicSize = 8.5f;
        Debug.Log("Shot Setup");
        main.depth = 1;
        house.depth = -1;
        ui.depth = 3;
        top.depth = 2;
        aim.depth = -1;

        main.rect = new Rect(new Vector2(0f, 0f), new Vector2(1f, 1f));
        vcam_ft.m_SoftZoneWidth = 0f;
        vcam.LookAt = launcher;
        vcam.Follow = launcher;
        vcam.enabled = true;
    }

    public void Trajectory()
    {
        //Debug.Log("Trajectory");
        main.depth = 1;
        house.depth = -1;
        ui.depth = 3;

        aim.orthographicSize = Mathf.Lerp(3.5f, 5f, (trajTarget.position.y) / 10f);

        if (trajTarget.position.y > 5.75f)
        {
            aim.depth = 3;
            aimMover.position = new Vector3(0f, 5.75f, 0f);
        }
        else if (trajTarget.position.y > 3.5f)
        {
            aim.depth = 3;
            aimMover.position = new Vector3(0f, trajTarget.position.y, 0f);
        }
        else if (trajTarget.position.y > 0f)
        {
            aim.depth = 3;
            aimMover.position = new Vector3(0f, 3.5f, 0f);
        }
        else
        {
            aim.depth = -1;
            aimMover.position = new Vector3(0f, 3.5f, 0f);
        }
        //vcam.LookAt = trajTarget;
        //vcam.Follow = trajTarget;
        //vcam.enabled = true;
    }

    public void Perspective()
    {
        if (persp.depth == 5)
        {
            persp.depth = -1;
        }
        else if (persp.depth == -1)
        {
            persp.depth = 5;
        }
    }

    public void RockFollow(Transform tFollowTarget)
    {
        Debug.Log("Rock Follow");
        vcam_ft.m_SoftZoneWidth = 2f;
        aim.depth = -1;
        main.depth = 1;
        house.depth = -1;
        top.depth = -1;
        ui.depth = 3;
        vcam.LookAt = tFollowTarget;
        vcam.Follow = tFollowTarget;
        vcam.enabled = true;
    }

    public void RockZoom(Transform tFollowTarget)
    {
        main.depth = 1;
        house.depth = -1;
        top.depth = -1;
        ui.depth = 3;
        vcam.LookAt = tFollowTarget;
        vcam.Follow = tFollowTarget;
        vcam.enabled = true;
        vcam.m_Lens.OrthographicSize = 3f;

        
    }
    public void ZoomOutTrack(float dist)
    {
        vcam.m_Lens.OrthographicSize = ((8.5f - 3.5f) * ((dist) / 3.5f)) + 3.5f;

    }
    public void InPlayZoom(float dist)
    {
        vcam.m_Lens.OrthographicSize = ((8.5f - 4.5f) * ((dist) / 12.5f)) + 4.5f;
        CinemachineFramingTransposer vcam_follow = FindObjectOfType<CinemachineFramingTransposer>();
        vcam_follow.m_ScreenY = (0.85f - 0.75f) * ((dist) / 12.5f) + 0.75f;
        //Debug.Log("vcam_follow.m_ScreenY is " + vcam_follow.m_ScreenY);
    }

    public void HouseView()
    {
        Debug.Log("House View");

        if (house.depth == 5)
        {
            ui.depth = 3;
            house.depth = -1;
        }
        else if (house.depth == -1)
        {
            ui.depth = 6;
            house.depth = 5;
        }
    }

    public void TopViewAuto()
    {
        Debug.Log("Top View Auto");

        if (top.depth == 2)
        {
            top.depth = -1;
        }
        else if (top.depth == -1)
        {
            top.depth = 2;
        }
    }

    public void TopView()
    {
        Debug.Log("Top View");

        if (top.depth == 2)
        {
            top.depth = -1;
        }
        else if (top.depth == -1)
        {
            top.depth = 2;
        }
    }
}
