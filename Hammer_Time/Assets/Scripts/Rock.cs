using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rock : MonoBehaviour
{
    public bool isStatic;
    public bool isReference;

    void Start()
    {
        if (isReference)
        {
            var shotScript = FindObjectOfType<Rock_TrajectoryLine>();
            shotScript.RegisterRock(this);
        }
    }
}
