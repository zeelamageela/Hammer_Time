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

    public Transform trajTarget;
    public Transform launcher;
    public GameManager gm;

    public GameObject vcam_go;
    public CinemachineVirtualCamera vcam;

    private void Start()
    {
        main.depth = 1;
        house.depth = -1;
        ui.depth = 2;
        top.depth = -1;
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
        Debug.Log("Shot Setup");
        main.depth = 1;
        house.depth = -1;
        ui.depth = 2;
        top.depth = -1;

        vcam.LookAt = launcher;
        vcam.Follow = launcher;
        vcam.enabled = true;
    }

    public void Trajectory()
    {
        Debug.Log("Trajectory");
        main.depth = 1;
        house.depth = -1;
        ui.depth = 2;

        vcam.LookAt = trajTarget;
        vcam.Follow = trajTarget;
        vcam.enabled = true;
    }

    public void RockFollow(Transform tFollowTarget)
    {
        Debug.Log("Rock Follow");

        main.depth = 1;
        house.depth = -1;
        ui.depth = 2;
        vcam.LookAt = tFollowTarget;
        vcam.Follow = tFollowTarget;
        vcam.enabled = true;
    }

    public void HouseView()
    {
        Debug.Log("House View");

        if (house.depth == 5)
        {
            house.depth = -1;
        }
        else if (house.depth == -1)
        {
            house.depth = 5;
        }
    }
    
    public void TopViewAuto()
    {
        Debug.Log("Top View");

        top.depth = 2;
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
