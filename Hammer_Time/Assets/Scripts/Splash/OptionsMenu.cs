using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public AudioManager am;
    public Slider volumeSlider;
    public float volume;

    private void Start()
    {
        //am.Play("Theme");
    }
    // Update is called once per frame
    void Update()
    {
        volume = volumeSlider.value;
        am.Volume("Theme", volume);
    }
}
