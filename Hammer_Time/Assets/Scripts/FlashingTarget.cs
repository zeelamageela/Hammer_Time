using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashingTarget : MonoBehaviour
{
    public Color targetColour;
    SpriteRenderer sr;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    // Update is called once per frame
    void Update()
    {
        sr.color = Color.Lerp(targetColour, Color.white, Mathf.PingPong(Time.time, 1));
    }
}
