using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowMotion
{
    float timeScale;

    public void SlowdownTime(bool slowMoOn)
    {
        if (slowMoOn == false)
        {
            timeScale = 1f;
        }
        else
        {
            timeScale = 0f;
        }

        Time.timeScale = timeScale;
    }
}
