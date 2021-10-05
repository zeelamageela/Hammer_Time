using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharColourChanger : MonoBehaviour
{
    public Color colour1;
    public Color colour2;

    public SpriteRenderer[] colour1GO;
    public SpriteRenderer[] colour2GO;

    GameSettingsPersist gsp;
    // Start is called before the first frame update
    void Start()
    {
        gsp = FindObjectOfType<GameSettingsPersist>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TeamColour(Color colour)
    {
        colour1 = colour;

        for (int i = 0; i < colour1GO.Length; i++)
        {
            colour1GO[i].color = colour1;
        }
    }
}
