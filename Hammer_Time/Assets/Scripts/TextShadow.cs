using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextShadow : MonoBehaviour
{
    public Text mainText;
    public Text shadowText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        shadowText.text = mainText.text;
    }
}
