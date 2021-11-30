using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextButton : MonoBehaviour
{

    public Text main;
    public Color clickedColour;
    public bool colourChange;

    Transform mainT;

    // Start is called before the first frame update
    void Start()
    {
        mainT = main.gameObject.transform;
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void Clicked()
    {
        main.rectTransform.anchoredPosition -= new Vector2 (15f, 15f);
        main.gameObject.GetComponent<Shadow>().effectDistance = Vector2.zero;

        if (colourChange)
            GetComponent<Image>().color = clickedColour;
    }
}
