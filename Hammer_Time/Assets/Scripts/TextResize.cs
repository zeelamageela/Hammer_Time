using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextResize : MonoBehaviour
{
    Text textField;

    public float charLimit;
    public float lengthLimit;
    public float fontSizeMin;
    public float fontSizeMax;

    private void Start()
    {
       textField = GetComponent<Text>();
    }
    // Update is called once per frame
    void Update()
    {
        if (textField.text.Length >= lengthLimit)
        {
            //Debug.Log("Text " + gameObject.transform.parent.transform.parent.name + " - " + textField.text.Length.ToString());
            //Debug.Log("Text Char Length is " + textField.text.Length);
            textField.fontSize = Mathf.RoundToInt( ((fontSizeMax - fontSizeMin) * ((textField.text.Length - lengthLimit) / (charLimit - lengthLimit))) + fontSizeMax);
        }
        else
            textField.fontSize = (int) fontSizeMax;
    }
}
