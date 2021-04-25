using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShooterAnim : MonoBehaviour
{
    private Animator anim;
    public GameObject square;
    public bool isPressed = false;
    public float pullback;
    int currentRock;
    GameObject rock;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
        
        anim.SetBool("mouseDown", isPressed);
        anim.SetFloat("Pullback", pullback);

    }
}
