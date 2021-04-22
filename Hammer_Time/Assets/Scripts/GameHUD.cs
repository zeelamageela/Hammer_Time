using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHUD : MonoBehaviour
{
    public Text Turn_Display;
    public Text rocksLeft_Display;
    public Slider rocks_Left;

    public void SetHUD(Rock_Info rock)
    {
        Turn_Display.text = rock.teamName + " Turn";
        rocksLeft_Display.text = rock.rockNumber.ToString();
        rocks_Left.maxValue = 8f;
        rocks_Left.value = rock.rockNumber;
    }

    public void RockCount(int rock_count)
    {
        rocks_Left.value = rock_count;
    }
}
