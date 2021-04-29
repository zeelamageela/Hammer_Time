using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteOpacity : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, .5f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
