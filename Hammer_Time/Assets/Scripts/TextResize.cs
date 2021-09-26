using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextResize : MonoBehaviour
{
    Text text;

    public float charLimit;
    public float lengthLimit;
    public float fontSizeMin;
    public float fontSizeMax;

    private void Start()
    {
       text = GetComponent<Text>();
    }
    // Update is called once per frame
    void Update()
    {
        if (text.text.Length >= lengthLimit)
        {
            //Debug.Log("Text " + gameObject.transform.parent.transform.parent.name + " - " + text.text.Length.ToString());
            text.fontSize = Mathf.RoundToInt( ((fontSizeMin - fontSizeMax) * ((text.text.Length - lengthLimit) / (charLimit - lengthLimit))) + fontSizeMax);
        }
        else
            text.fontSize = (int) fontSizeMax;
    }
}
