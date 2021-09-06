using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraManager : MonoBehaviour
{
    public Camera main;
    public Camera house;
    public Camera ui;
    public Camera top;
    public Camera persp;

    public Transform trajTarget;
    public Transform launcher;
    public GameManager gm;

    public GameObject vcam_go;
    public CinemachineVirtualCamera vcam;
    public CinemachineFramingTransposer vcam_ft;

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
    }
    public void ShotSetup()
    {
        vcam.enabled = false;
        vcam.m_Lens.OrthographicSize = 7.5f;
        Debug.Log("Shot Setup");
        main.depth = 1;
        house.depth = -1;
        ui.depth = 3;
        top.depth = 2;

        vcam_ft.m_SoftZoneWidth = 0f;
        vcam.LookAt = launcher;
        vcam.Follow = launcher;
        vcam.enabled = true;
    }

    public void Trajectory()
    {
        Debug.Log("Trajectory");
        main.depth = 1;
        house.depth = -1;
        ui.depth = 3;

        vcam.LookAt = trajTarget;
        vcam.Follow = trajTarget;
        vcam.enabled = true;
    }

    public void RockFollow(Transform tFollowTarget)
    {
        Debug.Log("Rock Follow");
        vcam_ft.m_SoftZoneWidth = 2f;
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
    public void InPlayZoom(float dist)
    {
        vcam.m_Lens.OrthographicSize = ((7.5f - 3.5f) * ((dist) / 6.5f)) + 3.5f;
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
