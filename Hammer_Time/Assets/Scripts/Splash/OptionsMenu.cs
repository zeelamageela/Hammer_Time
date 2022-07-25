using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TigerForge;

public class OptionsMenu : MonoBehaviour
{
    public AudioManager am;
    public Slider volumeSlider;
    public float volume;

    EasyFileSave myFile;


    private void Start()
    {
        am.PlayBG(4);
    }
    // Update is called once per frame
    void Update()
    {
        volume = volumeSlider.value;
        am.Volume(volume);
    }

    public void ClearHighScore()
    {
        myFile = new EasyFileSave("my_hiscore_data");

        myFile.Delete();
    }
}
